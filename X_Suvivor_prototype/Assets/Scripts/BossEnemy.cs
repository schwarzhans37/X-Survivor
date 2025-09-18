using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [Header("# Boss Behaviour")]
    public bool useMeleeWhenInRange = true;
    public float meleeRange = 1.2f;

    [Header("# Ranged Skills")]
    public RangedSkill fire;
    public RangedSkill magic;
    public RangedSkill lightning; // 보통 DirectToLockedPlayer 권장

    List<RangedSkill> skills;
    RangedSkill currentSkill;
    bool waitingForAnimEvent;

    void Start()
    {
        skills = new List<RangedSkill>();
        if (fire != null) skills.Add(fire);
        if (magic != null) skills.Add(magic);
        if (lightning != null) skills.Add(lightning);

        foreach (var s in skills)
        {
            if (s.startReady) s.ForceReady();
            else s.ResetCD();
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive || !isLive) return;
        if (anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit")) return;

        if (stunRemain > 0f) { stunRemain -= Time.fixedDeltaTime; if (stunRemain < 0f) stunRemain = 0f; }
        if (IsStunned) { rigid.velocity = Vector2.zero; return; }

        if (slowRemain > 0f) { slowRemain -= Time.fixedDeltaTime; if (slowRemain <= 0f) { slowRemain = 0f; slowMultiplier = 1f; } }

        if (!isAttacking)
        {
            float dist = target ? Vector2.Distance(target.position, transform.position) : 999f;
            bool canMelee = useMeleeWhenInRange && attackHitbox && dist <= meleeRange;

            if (canMelee)
            {
                StartCoroutine(DoMeleeOnce());
            }
            else
            {
                var ready = GetReadySkills();
                if (ready.Count > 0)
                {
                    var skill = ready[Random.Range(0, ready.Count)];
                    StartCoroutine(AttackRoutine(skill));
                }
            }
        }

        // 추적 이동
        float curSpeed = baseSpeed * slowMultiplier;
        Vector2 dirVec = target.position - rigid.position;
        rigid.MovePosition(rigid.position + dirVec.normalized * curSpeed * Time.fixedDeltaTime);
        rigid.velocity = Vector2.zero;
    }

    // 기본 Enemy의 근접 루틴은 사용 안 함
    protected override IEnumerator AttackRoutine() { yield break; }

    public IEnumerator AttackRoutine(RangedSkill skill)
    {
        isAttacking = true;
        yield return StartCoroutine(CastAndFire(skill));
        skill.nextReadyTime = Time.time + skill.cooldown;

        // 필요 시 후딜
        yield return new WaitForSeconds(3.0f);

        isAttacking = false;
    }

    IEnumerator DoMeleeOnce()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        if (attackHitbox) attackHitbox.enabled = false;

        if (anim) { anim.ResetTrigger("Hit"); anim.SetTrigger("Attack"); }

        // Attack 상태 진입 대기(최대 0.25s)
        float guard = 0f;
        while (guard < 0.25f)
        {
            if (!anim || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) break;
            guard += Time.deltaTime; yield return null;
        }
        // Attack 상태 종료까지 대기
        while (anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) yield return null;

        if (attackHitbox) attackHitbox.enabled = false;

        isAttacking = false;
    }

    List<RangedSkill> GetReadySkills()
    {
        float now = Time.time;
        var list = new List<RangedSkill>(skills.Count);
        foreach (var s in skills)
            if (s != null && now >= s.nextReadyTime)
                list.Add(s);
        return list;
    }

    IEnumerator CastAndFire(RangedSkill s)
    {
        if (anim && !string.IsNullOrEmpty(s.animTrigger))
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger(s.animTrigger);
        }

        if (s.preCastDelay > 0f)
            yield return new WaitForSeconds(s.preCastDelay);

        if (s.fireOnAnimEvent)
        {
            currentSkill = s;
            waitingForAnimEvent = true;

            // 안전 타임아웃
            float timeout = 2.0f;
            while (waitingForAnimEvent && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (waitingForAnimEvent) Debug.LogError("Animation Event Timeout!");
            yield break;
        }

        yield return StartCoroutine(ExecutePattern(s));
    }

    // === 패턴 실행 (슬림) ===
    IEnumerator ExecutePattern(RangedSkill s)
    {
        Transform[] pivots = (s.shootPivots != null && s.shootPivots.Length > 0)
            ? s.shootPivots
            : new Transform[] { shootPivot ? shootPivot : transform };

        // Direct 모드면 발사 순간의 플레이어 좌표를 잠가둔다
        Vector3 lockedPos = Vector3.zero;
        if (s.pattern == RangedSkill.PatternMode.DirectToLockedPlayer && GameManager.instance?.player)
            lockedPos = GameManager.instance.player.transform.position;

        foreach (var p in pivots)
        {
            Vector3 origin = p.position;
            Vector2 shotDir;

            if (s.pattern == RangedSkill.PatternMode.DirectToLockedPlayer)
            {
                // 잠근 좌표로 직사(유도 X)
                Vector3 targetPos = GameManager.instance?.player ? lockedPos : origin + Vector3.right;
                shotDir = ((Vector2)(targetPos - origin)).normalized;
            }
            else // AimedAtPlayer
            {
                Vector3 targetPos = GameManager.instance?.player
                    ? GameManager.instance.player.transform.position
                    : origin + Vector3.right;
                shotDir = ((Vector2)(targetPos - origin)).normalized;
            }

            SpawnProjectile(origin, shotDir, s.projectilePoolIndex, s.projectileAnimName);
        }

        yield break;
    }

    void SpawnProjectile(Vector3 origin, Vector2 dir, int poolIndex, string animName)
    {
        var go = GameManager.instance.pool.Get(PoolCategory.Projectile, poolIndex);
        go.transform.position = origin;
        go.transform.rotation = Quaternion.identity; // 그래픽 회전은 EnemyProjectile이 처리

        var proj = go.GetComponent<EnemyProjectile>();
        if (!proj) return;

        proj.Fire(dir, animName);
    }

    // 애니메이션 이벤트 연동
    public override void AE_EnableHitbox() { base.AE_EnableHitbox(); }
    public override void AE_DisableHitbox() { base.AE_DisableHitbox(); }

    public override void AE_FireProjectile()
    {
        if (waitingForAnimEvent && currentSkill != null)
        {
            StartCoroutine(ExecutePattern(currentSkill));
            waitingForAnimEvent = false;
            currentSkill = null;
            return;
        }
        base.AE_FireProjectile();
    }
}

[System.Serializable]
public class RangedSkill
{
    public enum PatternMode
    {
        DirectToLockedPlayer, // 발사 "순간"의 플레이어 좌표로 직사
        AimedAtPlayer         // 발사 시점의 플레이어 방향으로 단발
    }

    [Header("Common")]
    public string name;
    public string animTrigger = "Fire";
    public float cooldown = 4f;
    public bool startReady = true;
    public float preCastDelay = 0.25f;
    public bool fireOnAnimEvent = false;

    [Header("Projectile")]
    public int projectilePoolIndex = 0;
    public string projectileAnimName = "Anim";
    public Transform[] shootPivots; // 비우면 BossEnemy.shootPivot 사용

    [Header("Pattern")]
    public PatternMode pattern = PatternMode.AimedAtPlayer;

    [HideInInspector] public float nextReadyTime;
    public void ResetCD() => nextReadyTime = Time.time + cooldown;
    public void ForceReady() => nextReadyTime = Time.time;
}
