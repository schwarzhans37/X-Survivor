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
    /* 
        물리(Physics)계산용 업데이트 함수
        플레이어의 위치 이동을 위해 사용됨
        - 프레임 속도 영향 안받음(컴퓨터 성능 영향 안받음)
        - 고정된 시간 간격으로 호출함
        - Rigidbody
     */
    {
        if (!GameManager.instance.isLive) {
            return;
        }
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    void LateUpdate()
    /* 
        한 프레임의 모든 Update 함수가 실행 된 후 호출
        오브젝트의 이동, 로직 등의 처리가 끝난 최종 결과를 가지고 사용
        - 프레임 속도에 따라 가변적임
        - 애니메이션 후처리, 카메라 추적 등 사용
     */
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

    public void Dead()
    {
        gameObject.SetActive(false);
    }
}
