using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy : MonoBehaviour
{
    [Header("# Monster's Status")]
    public float speed; // 몬스터 이동속도
    public float health;    // 몬스터의 현재 체력
    public float maxHealth; // 몬스터의 최대 체력

    [Header("# Data")]
    public MonsterData monsterData;

    [Header("# Monster's Physics")]
    public Rigidbody2D target;

    bool isLive;
    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    // -------- 슬로우 상태 (추가) --------
    float baseSpeed;            // baseSpeed : 원래 이동속도(Init에서 기록)
    float slowMultiplier = 1f;  // slowMultiplier : 1=정상, 0.7=30% 감속
    float slowRemain = 0f;      // slowRemain : 남은 슬로우 시간(0이 되면 자동 정상화)

    // -------- 스턴(기절) 상태 --------
    float stunRemain = 0f;             // 남은 스턴 시간
    public bool IsStunned => stunRemain > 0f; // 외부 조회용
    public bool IsAlive => isLive;           // 외부 조회용(타겟 선택 등)

    void Awake()
    // 초기화(선언)
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);

        // OnEnable 될 때마다 monsterData를 기반으로 스탯을 초기화
        Init(monsterData);
    }
        
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        // ----- 스턴 시간 갱신 -----
        if (stunRemain > 0f)
        {
            stunRemain -= Time.fixedDeltaTime;
            if (stunRemain < 0f) stunRemain = 0f;
        }

        // 스턴 중에는 이동 완전 정지
        if (IsStunned)
        {
            rigid.velocity = Vector2.zero;
            return;
        }
        
        // ----- 슬로우 시간 처리 (추가) -----
        if (slowRemain > 0f)
        {
            slowRemain -= Time.fixedDeltaTime;
            if (slowRemain <= 0f)
            {
                slowRemain = 0f;
                slowMultiplier = 1f; // 만료 시 정상화
            }
        }

        // 현재 속도 = 원속도 * 슬로우 멀티
        float curSpeed = baseSpeed * slowMultiplier;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * curSpeed * Time.fixedDeltaTime; // curSpeed로 변경
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive) {
            return;
        }
        if (!isLive)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    public void Init(MonsterData data)
    {
        // ScriptableObject에서 데이터를 가져와 적용
        monsterData = data; // 데이터 참조 저장
        speed = data.Speed;
        maxHealth = data.Maxhealth;
        health = maxHealth;

        // 애니메이터 컨트롤러 설정
        if (data.animator != null)
            anim.runtimeAnimatorController = data.animator;

        // ----- 슬로우 상태 리셋 (추가) -----
        baseSpeed = speed;        // 원래 속도 저장
        slowMultiplier = 1f;
        slowRemain = 0f;
        stunRemain = 0f;     // 스턴 해제
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;
        
        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine("KnockBack");

        if (health > 0) {       // 살아있음 => 피격 반응
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else {         // 죽음
            Die();
        }
    }

    void DropItems()
    {
        if (monsterData == null) return;

        // 1. 경험치 구슬 드랍
        // min과 max가 같으면 고정값, 다르면 랜덤
        int expOrbCount = Random.Range(monsterData.mixExpOrbs, monsterData.maxExpOrbs + 1);
        int expPoolIndex = 0; // 기본 경험치 구슬 인덱스
        switch (monsterData.tier)
        {
            case MonsterData.MonsterTier.Elite:
                expPoolIndex = 1; // Exp2
                break;
            case MonsterData.MonsterTier.Boss:
                expPoolIndex = 2; // Exp3
                break;
        }

        for (int i = 0; i < expOrbCount; i++)
        {
            GameObject item = GameManager.instance.pool.Get(PoolCategory.Item, expPoolIndex);
            // 아이템이 겹치지 않게 살짝 랜덤한 위치에 생성
            item.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
        }

        // 2. 추가 아이템 리스트 드랍 (골드, 젬 등)
        foreach (var itemToDrop in monsterData.dropList)
        {
            // 드랍 확률 체크
            if (Random.Range(0f, 1f) <= itemToDrop.dropChance)
            {
                int amount = Random.Range(itemToDrop.minAmount, itemToDrop.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                {
                    GameObject item = GameManager.instance.pool.Get(PoolCategory.Item, itemToDrop.itemPoolIndex);
                    item.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
                }
            }
        }
    }

    // 비동기 코루틴 함수
    IEnumerator KnockBack()
    {
        yield return wait;      // 다음 하나의 물리 프레임까지 기다리는 딜레이
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
    }

    //소모성 스킬용 공용 데미지 함수 추가
    public void ApplyDamage(float amount, Vector3? sourcePos = null, float knockPower = 3f)
    {
        if (!isLive) return;

        // 체력 감소
        health -= amount;

        // 넉백: sourcePos가 있으면 그 기준으로 밀기(폭발 등), 없으면 플레이어 기준(기존)
        if (sourcePos.HasValue)
            StartCoroutine(KnockBackFrom(sourcePos.Value, knockPower));
        else
            StartCoroutine("KnockBack");

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            Die();
        }
    }

    // 폭발 중심에서 바깥으로 밀어내는 넉백
    IEnumerator KnockBackFrom(Vector3 sourcePos, float power)
    {
        yield return wait; // 물리 프레임 싱크
        Vector3 dirVec = (transform.position - sourcePos).normalized;
        rigid.AddForce(dirVec * power, ForceMode2D.Impulse);
    }

    // ===== 슬로우 적용 (자기장용) =====
    // percent: 0.3f -> 30% 감속, duration: 유지 시간(틱마다 갱신 가능)
    public void ApplySlow(float percent, float duration)
    {
        float m = Mathf.Clamp01(1f - percent); // 1→정상, 0.7→30%감속
        // 더 강한(작은) 멀티가 오면 갱신
        if (m < slowMultiplier) slowMultiplier = m;
        // 남은 시간은 큰 값으로 갱신(연장)
        if (duration > slowRemain) slowRemain = duration;
    }

    // === 스턴(낙뢰용) ===
    public void ApplyStun(float duration)
    {
        if (!isLive) return;
        if (duration > stunRemain) stunRemain = duration; // 더 긴 스턴으로 갱신
        // 연출을 원하면 여기서 anim.SetTrigger("Hit") 등 추가 가능
        rigid.velocity = Vector2.zero; // 즉시 정지
    }

    void Die()
    {
        isLive = false;
        coll.enabled = false;
        rigid.simulated = false;
        spriter.sortingOrder = 1;
        anim.SetBool("Dead", true);
        GameManager.instance.AddKill();

        DropItems();

        if (monsterData.tier == MonsterData.MonsterTier.Boss)
        {
            GameManager.instance.NotifyBossDefeated();
        }

        if (GameManager.instance.isLive)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    public void Dead()
    {
        gameObject.SetActive(false);
    }
}
