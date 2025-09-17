using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy 클래스를 상속받습니다.
public class BossEnemy : Enemy
{
    [Header("# Boss Behaviour")]
    public bool useMeleeWhenInRange = true;
    public float meleeRange = 1.2f;

    [Header("# Ranged Skills")]
    public RangedSkill fire;    // Trigger: "Fire"
    public RangedSkill magic;   // Trigger: "Magic"
    public RangedSkill lightning;   // Trigger: "Lightning"

    List<RangedSkill> skills;

    // 애니 이벤트 연동용
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
        if (!GameManager.instance.isLive) return;
        if (!isLive) return;
        if (!isLive || (anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit")))
            return;

        if (stunRemain > 0f) { stunRemain -= Time.fixedDeltaTime; if (stunRemain < 0f) stunRemain = 0f; }
        if (IsStunned) { rigid.velocity = Vector2.zero; return; }

        if (slowRemain > 0f) { slowRemain -= Time.fixedDeltaTime; if (slowRemain <= 0f) { slowRemain = 0f; slowMultiplier = 1f; } }

        if (!isAttacking)
        {
            float dist = (target ? Vector2.Distance(target.position, transform.position) : 999f);
            bool canMelee = useMeleeWhenInRange && attackHitbox != null && dist <= meleeRange;

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

        float curSpeed = baseSpeed * slowMultiplier;
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * curSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    protected override IEnumerator AttackRoutine() { yield break; }

    public IEnumerator AttackRoutine(RangedSkill skill)
    {
        isAttacking = true;
        yield return StartCoroutine(CastAndFire(skill));
        skill.nextReadyTime = Time.time + skill.cooldown;

        yield return new WaitForSeconds(3.0f);

        isAttacking = false;
    }

    IEnumerator DoMeleeOnce()
    {
        if (attackHitbox) attackHitbox.enabled = false;

        if (anim)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Attack");
        }

        float guard = 0f;
        while (guard < 0.25f)
        {
            if (!anim || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) break;
            guard += Time.deltaTime; yield return null;
        }
        while (anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) yield return null;

        if (attackHitbox) attackHitbox.enabled = false;

        isAttacking = false;
        yield return null;
    }

    List<RangedSkill> GetReadySkills()
    {
        float now = Time.time;
        var list = new List<RangedSkill>();
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

        if (s.preCastDelay > 0f) yield return new WaitForSeconds(s.preCastDelay);

        if (s.fireOnAnimEvent)
        {
            currentSkill = s;
            waitingForAnimEvent = true;

            float timeout = 2.0f;
            while (waitingForAnimEvent && (timeout > 0f))
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (waitingForAnimEvent) Debug.LogError("Animation Event Timeout!");
            yield break;
        }

        yield return StartCoroutine(ExecutePattern(s));
    }

    IEnumerator ExecutePattern(RangedSkill s)
    {
        Transform[] pivots = (s.shootPivots != null && s.shootPivots.Length > 0)
            ? s.shootPivots
            : new Transform[] { shootPivot != null ? shootPivot : transform };

        float aimBase = 0f;
        if (s.aimAtPlayer && GameManager.instance?.player)
        {
            Vector3 origin = shootPivot ? shootPivot.position : transform.position;
            Vector2 toP = (GameManager.instance.player.transform.position - origin);
            aimBase = Mathf.Atan2(toP.y, toP.x) * Mathf.Rad2Deg;
        }

        float angle = s.startAngleDeg;

        for (int b = 0; b < s.bursts; b++)
        {
            float baseAngle = aimBase + angle;

            int count = Mathf.Max(1, s.projectilesPerBurst);
            float step = (s.spreadDeg >= 360f) ? 360f / count
                                                : (count > 1 ? s.spreadDeg / (count - 1) : 0f);
            float start = (s.spreadDeg >= 360f) ? 0f : -s.spreadDeg * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float a = baseAngle + start + step * i + Random.Range(-s.randomJitterDeg, s.randomJitterDeg);
                Vector2 dir = new(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));

                foreach (var p in pivots)
                    SpawnProjectile(p.position, dir, s.projectilePoolIndex, s.overrideProjectile, s.projectileAnimName);
            }

            if (b < s.bursts - 1) yield return new WaitForSeconds(s.burstInterval);

            angle += s.rotatePerBurstDeg;
        }
    }

    void SpawnProjectile(Vector3 origin, Vector2 dir, int poolIndex, ProjectileOverride ov, string animName)
    {
        var go = GameManager.instance.pool.Get(PoolCategory.Projectile, poolIndex);
        go.transform.position = origin;
        go.transform.rotation = Quaternion.identity; // 회전 초기화

        var proj = go.GetComponent<EnemyProjectile>();
        if (!proj) return;

        if (ov != null) ov.ApplyTo(proj);
        proj.Fire(dir, animName);
    }

    public override void AE_EnableHitbox()
    {
        base.AE_EnableHitbox();
    }

    public override void AE_DisableHitbox()
    {
        base.AE_DisableHitbox();
    }

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
    [Header("Common")]
    public string name;
    public string animTrigger = "Fire";
    public float cooldown = 4f;
    public bool startReady = true;
    public float preCastDelay = 0.25f;
    public bool fireOnAnimEvent = false;

    [Header("Projectile")]
    public int projectilePoolIndex = 0;
    public string projectileAnimName = "투사체 애니메이션 이름 입력";
    public Transform[] shootPivots;

    [Header("Pattern")]
    public bool aimAtPlayer = true;
    public float startAngleDeg = 0f;
    public float spreadDeg = 360f;
    public int projectilesPerBurst = 12;
    public int bursts = 1;
    public float burstInterval = 0.12f;
    public float rotatePerBurstDeg = 0f;
    public float randomJitterDeg = 0f;

    public ProjectileOverride overrideProjectile;

    [HideInInspector] public float nextReadyTime;
    public void ResetCD() => nextReadyTime = Time.time + cooldown;
    public void ForceReady() => nextReadyTime = Time.time;
}

[System.Serializable]
public class ProjectileOverride
{
    public bool overrideDamage; public int damage;
    public bool overrideSpeed; public float speed;
    public bool overrideLife; public float life;

    public void ApplyTo(EnemyProjectile p)
    {
        if (overrideDamage) p.damage = damage;
        if (overrideSpeed) p.speed = speed;
        if (overrideLife) p.lifeTime = life;
    }
}