using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy : MonoBehaviour
{
    [Header("# Monster's Status")]
    public float speed;           // 이동속도(실제 이동에 씀)
    public float health;          // 현재 체력
    public float maxHealth;       // 최대 체력

    [Header("# Data")]
    public MonsterData monsterData;   // 스탯/드랍/애니 컨트롤러 등 기존 SO

    [Header("# Monster's Physics")]
    public Rigidbody2D target;        // 플레이어 Rigidbody

    [Header("# Monster Attack")]
    public float attackRange = 1.1f;    // 공격 시작 거리
    public float attackCooldown = 1.0f; // 공격 쿨
    public Collider2D attackHitbox;     // Graphics 자식(Trigger)

    [Header("# Graphics & Colliders")]
    public Transform graphics;                // Sprite/Shadow/Hitbox 묶음(이것만 좌우 반전)
    public Transform shadow;                  // 선택
    public CapsuleCollider2D bodyCollider;    // 루트 몸통(비트리거)

    [Header("Body Collider per-direction tweak")]
    public float bodyRightX = 0f; // 오른쪽 볼 때 X 추가 오프셋(+면 앞쪽)
    public float bodyLeftX = 0f; // 왼쪽 볼 때 X 추가 오프셋(+면 앞쪽)
    public float bodyY = 0f; // 위/아래 공통 보정이 필요하면 사용(선택)

    [Header("# Ranged Attack")]
    public Transform shootPivot;           // 총구 위치(Enemy 프리팹에서 지정)
    public int projectilePoolIndex = 0;    // PoolManager.Projectile 배열 인덱스


    // 내부 저장: 기본 위치/오프셋
    [SerializeField] Vector2 graphicsBaseLocalPos; // 비워두면 Awake에서 자동 기록
    Vector3 shadowBaseLocalPos;
    Vector2 bodyColBaseOffset;
    Vector2 atkBoxBaseOffset;

    // 상태
    protected bool isAttacking = false;
    protected float lastAttackTime = -999f;

    protected bool isLive;
    protected Rigidbody2D rigid;
    Collider2D coll;
    protected Animator anim;
    protected SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    // 슬로우/스턴
    protected float baseSpeed;
    protected float slowMultiplier = 1f;
    protected float slowRemain = 0f;
    protected float stunRemain = 0f;
    public bool IsStunned => stunRemain > 0f;
    public bool IsAlive => isLive;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        // Animator/SpriteRenderer는 Graphics에 있음
        anim = GetComponentInChildren<Animator>(true);
        spriter = anim ? anim.GetComponent<SpriteRenderer>()
                       : GetComponentInChildren<SpriteRenderer>(true);

        if (bodyCollider == null) bodyCollider = GetComponent<CapsuleCollider2D>();

        // 기준값 항상 기록(0,0도 포함)
        if (attackHitbox is BoxCollider2D box) atkBoxBaseOffset = box.offset;
        if (bodyCollider) bodyColBaseOffset = bodyCollider.offset;

        if (graphics && graphicsBaseLocalPos == Vector2.zero)
            graphicsBaseLocalPos = graphics.localPosition;

        if (shadow) shadowBaseLocalPos = shadow.localPosition;

        wait = new WaitForFixedUpdate();
    }

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;

        coll.enabled = true;
        rigid.simulated = true;

        if (spriter) spriter.sortingOrder = 2;
        if (anim) anim.SetBool("Dead", false);

        Init(monsterData);

        if (attackHitbox) attackHitbox.enabled = false;

        // 히트박스에 부모 참조(넉백 방향용)
        var hb = attackHitbox ? attackHitbox.GetComponent<EnemyAttackHitbox>() : null;
        if (hb) hb.attacker = transform;
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive) return;
        if (!isLive) return;

        if (!isLive || (anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit")))
            return;

        // 스턴은 그대로(스턴 중엔 멈춤)
        if (stunRemain > 0f) { stunRemain -= Time.fixedDeltaTime; if (stunRemain < 0f) stunRemain = 0f; }
        if (IsStunned) { rigid.velocity = Vector2.zero; return; }

        if (slowRemain > 0f) { slowRemain -= Time.fixedDeltaTime; if (slowRemain <= 0f) { slowRemain = 0f; slowMultiplier = 1f; } }

        // ── 공격 체크: "트리거만" 걸고 이동은 계속 ──
        float dist = Vector2.Distance(target.position, rigid.position);
        bool canAttack = dist <= attackRange &&
                         Time.time >= lastAttackTime + attackCooldown &&
                         !isAttacking;

        if (canAttack)
        {
            StartCoroutine(AttackRoutine());
        }

        // 이동
        float curSpeed = baseSpeed * slowMultiplier;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * curSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive) return;
        if (!isLive) return;
        if (target == null) return;

        // 1) 좌/우 방향
        float sign = (target.position.x < rigid.position.x) ? -1f : 1f;

        // 2) 그래픽 그룹만 좌우 반전 (자식 전부 함께 뒤집힘)
        if (graphics) graphics.localScale = new Vector3(sign, 1f, 1f);

        // 3) 루트 몸통 콜라이더만 오프셋 미러링 (루트는 스케일 고정이니까)
        if (bodyCollider)
        {
            float baseX = Mathf.Abs(bodyColBaseOffset.x) * sign;
            float addX = (sign > 0f) ? bodyRightX : -bodyLeftX; // 좌우 각각 따로
            bodyCollider.offset = new Vector2(baseX + addX,
                                              bodyColBaseOffset.y + bodyY);
        }

        // 4) AttackHitbox / Shadow 는 graphics 밑에 있으므로
        //    추가 보정(오프셋/위치) 하지 않는다. (double flip 방지)
        //    => 애니에서 설정한 offset/size 그대로 사용 + 부모 스케일로 자동 반전
    }

    public void Init(MonsterData data)
    {
        monsterData = data;

        // 기본 스탯
        speed = data.Speed;
        maxHealth = data.Maxhealth;
        health = maxHealth;

        // 애니메이터 교체(개별 몬스터 컨트롤러)
        if (data.animator != null && anim) anim.runtimeAnimatorController = data.animator;

        // 상태 리셋
        baseSpeed = speed;
        slowMultiplier = 1f;
        slowRemain = 0f;
        stunRemain = 0f;
        isAttacking = false;
        lastAttackTime = -999f;
        if (attackHitbox) attackHitbox.enabled = false;

        // ※ 방향 보정값은 Enemy.cs의 facingAdjust(프리팹별)에서 사용
        // (MonsterData에 넣지 않아도 됩니다)
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive) return;
        if (!collision.CompareTag("Bullet")) return;

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            if (anim) anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx("Hit");
        }
        else
        {
            Die();
        }
    }

    // ===== 공격 루틴 & 애니 이벤트 백업 =====
    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // 안전: 시작 전에 꺼두기
        if (attackHitbox) attackHitbox.enabled = false;

        // 애니 트리거
        if (anim)
        {
            anim.ResetTrigger("Hit");   // 피격 트리거 충돌 방지(선택)
            anim.SetTrigger("Attack");
        }

        // 1) 실제로 Attack 상태에 들어갈 때까지 잠깐 대기(최대 0.25s)
        float guard = 0f;
        while (guard < 0.25f)
        {
            if (!anim || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                break;

            guard += Time.deltaTime;
            yield return null;
        }

        // 2) Attack 상태가 끝날 때까지 대기
        while (anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            yield return null;

        // 안전: 끝나면 반드시 꺼두기(이벤트 누락 대비)
        if (attackHitbox) attackHitbox.enabled = false;

        // 3) 남은 쿨타임 보장
        float next = lastAttackTime + attackCooldown;
        if (Time.time < next)
            yield return new WaitForSeconds(next - Time.time);

        isAttacking = false;
    }

    // 애니메이션 이벤트용
    public virtual void AE_FireProjectile()
    {
        if (!GameManager.instance?.player) return;

        // 1) 발사 원점과 목표 방향
        Vector3 origin = shootPivot ? shootPivot.position : transform.position;
        Vector3 toPlayer = (GameManager.instance.player.transform.position - origin).normalized;

        // 2) 풀에서 투사체 꺼내 배치
        var go = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, projectilePoolIndex);
        go.transform.position = origin;

        // 3) 진행 방향 전달 → 투사체가 스스로 움직임
        var proj = go.GetComponent<EnemyProjectile>();
        if (proj) proj.Fire(toPlayer, "Fly");
    }

    // 애니메이션 이벤트용
    public virtual void AE_EnableHitbox() { if (attackHitbox) attackHitbox.enabled = true; Debug.Log("히트박스 활성화됨!"); }
    public virtual void AE_DisableHitbox() { if (attackHitbox) attackHitbox.enabled = false; Debug.Log("히트박스 비활성화됨!"); }

    void DropItems()
    {
        if (monsterData == null) return;

        int expOrbCount = Random.Range(monsterData.mixExpOrbs, monsterData.maxExpOrbs + 1);
        int expPoolIndex = 0;
        switch (monsterData.tier)
        {
            case MonsterData.MonsterTier.Elite: expPoolIndex = 1; break;
            case MonsterData.MonsterTier.Boss: expPoolIndex = 2; break;
        }

        for (int i = 0; i < expOrbCount; i++)
        {
            GameObject item = GameManager.instance.pool.Get(PoolManager.PoolCategory.Item, expPoolIndex);
            item.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
        }

        foreach (var itemToDrop in monsterData.dropList)
        {
            if (Random.Range(0f, 1f) <= itemToDrop.dropChance)
            {
                int amount = Random.Range(itemToDrop.minAmount, itemToDrop.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                {
                    GameObject item = GameManager.instance.pool.Get(PoolManager.PoolCategory.Item, itemToDrop.itemPoolIndex);
                    item.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
                }
            }
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);
    }

    public void ApplyDamage(float amount, Vector3? sourcePos = null, float knockPower = 3f)
    {
        if (!isLive) return;

        health -= amount;

        if (sourcePos.HasValue) StartCoroutine(KnockBackFrom(sourcePos.Value, knockPower));
        else StartCoroutine(KnockBack());

        if (health > 0)
        {
            if (anim) anim.SetTrigger("Hit");
            if (GameManager.instance.isLive) AudioManager.instance.PlaySfx("Hit");
        }
        else
        {
            Die();
        }
    }

    IEnumerator KnockBackFrom(Vector3 sourcePos, float power)
    {
        yield return wait;
        Vector3 dirVec = (transform.position - sourcePos).normalized;
        rigid.AddForce(dirVec * power, ForceMode2D.Impulse);
    }

    // 슬로우/스턴
    public void ApplySlow(float percent, float duration)
    {
        float m = Mathf.Clamp01(1f - percent);
        if (m < slowMultiplier) slowMultiplier = m;
        if (duration > slowRemain) slowRemain = duration;
    }

    public void ApplyStun(float duration)
    {
        if (!isLive) return;
        if (duration > stunRemain) stunRemain = duration;
        rigid.velocity = Vector2.zero;
    }

    void Die()
    {
        isLive = false;

        if (attackHitbox) attackHitbox.enabled = false;

        coll.enabled = false;
        rigid.simulated = false;

        if (spriter) spriter.sortingOrder = 1;
        if (anim) anim.SetBool("Dead", true);

        GameManager.instance.AddKill();
        DropItems();

        if (monsterData.tier == MonsterData.MonsterTier.Boss)
            GameManager.instance.NotifyBossDefeated();

        if (GameManager.instance.isLive)
            AudioManager.instance.PlaySfx("Dead");

        // Death 끝에서 Dead()를 애니메이션 이벤트/SMB로 호출하길 권장
        // 필요시 코루틴으로 딜레이 후 Dead() 호출해도 됨
    }

    public void Dead()
    {
        gameObject.SetActive(false);
    }
}
