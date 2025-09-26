using System.Collections;
using System.Linq;
using UnityEngine;

public class LightningSkill : MonoBehaviour
{
    [Header("기본")]
    public float cooldown = 12f;         // 쿨타임
    public float cooldownTimer;
    public int strikeCount = 5;          // 총 타격 횟수
    public float strikeInterval = 0.5f;  // 타격 간격
    public LayerMask enemyMask;          // Enemy 레이어

    [Header("피해/판정")]
    public int damage = 40;             // 강한 피해
    public float radius = 1.2f;          // 충돌 반경(데미지/스턴 적용 범위)
    public float stunDuration = 1.0f;    // 스턴 시간

    [Header("이펙트")]
    public GameObject lightningFxPrefab; // 낙뢰 VFX 프리팹(1회 재생)
    public float fxLifetime = 0.8f;      // VFX 생존 시간
    public bool scaleFxToRadius = true;  // 반경에 맞춰 VFX 자동 스케일

    const float FX_SCALE_MULT = 6f;     // 실제 뇌격 애니메이션 크기
    const float DAMAGE_R_MULT = 3f;     // 실제 피해 반경 범위

    // ===== 상단 필드에 추가 =====
    [Header("SFX")]
    public AudioClip strikeSfx;                               // 번개 1타 사운드
    [Range(0.25f, 2f)] public float strikeSfxPitch = 1.2f;    // 살짝 빠르게
    [Range(0f, 1.5f)] public float strikeSfxVolume = 1.0f;   // 0~1.5
    [Tooltip("같은 소리가 과도하게 겹치는 것을 막는 최소 간격(초)")]
    public float sfxMinInterval = 0.08f;                      // 짧은 중복 억제
    [Tooltip("0이면 전체 재생, 0보다 크면 앞부분만 잘라 재생")]
    public float strikeSfxMaxDuration = 0.35f;                // 긴 파일 앞부분만
    private float _lastSfxTime;                               // 내부 타임스탬프



    bool casting;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public bool TryUse()
    {
        if (casting) return false;
        if (cooldownTimer > 0f) return false;

        // 살아있는 적이 없으면 시전 안 함(원하면 시전하도록 바꿔도 됨)
        if (FindAliveEnemies().Length == 0) return false;

        StartCoroutine(CoCast());
        return true;
    }

    public float GetCooldownRatio()
    {
        return Mathf.Clamp01(cooldownTimer / cooldown);
    }

    IEnumerator CoCast()
    {
        casting = true;
        cooldownTimer = cooldown;

        for (int i = 0; i < strikeCount; i++)
        {
            var target = FindClosestEnemy(); // 가장 가까운 적으로 변경
            if (target != null)
            {
                Vector3 hitPos = target.transform.position;

                // 1) 이펙트 소환
                SpawnFxAt(hitPos);

                // 2) 데미지/스턴 적용
                ApplyAOE(hitPos);

                // 3) 사운드
                PlayStrikeSfx();
            }

            // 다음 타격까지 대기
            if (i < strikeCount - 1)
                yield return new WaitForSeconds(strikeInterval);
        }

        casting = false;
    }

    void SpawnFxAt(Vector3 pos)
    {
        if (lightningFxPrefab == null) return;

        var fx = Instantiate(lightningFxPrefab, pos, Quaternion.identity);

        if (scaleFxToRadius)
        {
            var sr = fx.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float w = sr.sprite.bounds.size.x;
                if (w > 0f)
                {
                    float scale = (radius * 2f * FX_SCALE_MULT) / w;
                    fx.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }

        Destroy(fx, fxLifetime);
    }

    void ApplyAOE(Vector3 center)
    {
        float damageRadius = radius * DAMAGE_R_MULT;

        var cols = Physics2D.OverlapCircleAll(center, damageRadius, enemyMask);
        foreach (var c in cols)
        {
            if (c.TryGetComponent<Enemy>(out var e) && e.IsAlive)
            {
                e.ApplyDamage(damage, center, 0f);
                e.ApplyStun(stunDuration);
            }
        }
    }

    // ===== 헬퍼 함수 교체 =====
    void PlayStrikeSfx()
    {
        if (!strikeSfx || AudioManager.instance == null) return;

        // 중복 재생 억제
        if (Time.time - _lastSfxTime < sfxMinInterval) return; // 너무 촘촘한 중복 억제
        _lastSfxTime = Time.time;

        AudioManager.instance.PlaySfx("Lightning");
    }


    // 플레이어 기준 '가장 가까운' 적 찾기
    Enemy FindClosestEnemy()
    {
        var enemies = FindAliveEnemies();
        if (enemies.Length == 0) return null;

        Vector3 p = transform.position;
        Enemy best = null;
        float bestSqr = float.PositiveInfinity;

        // 성능: 거리 비교는 sqrt 없는 제곱거리로
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            Vector3 d = e.transform.position - p;
            float sqr = d.sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = e;
            }
        }
        return best;
    }

    // 살아있는 적 목록(씬 전역). 적이 매우 많아지면 매니저/풀에서 관리 권장.
    Enemy[] FindAliveEnemies()
    {
        return GameObject.FindObjectsOfType<Enemy>(false)
            .Where(e => e != null && e.gameObject.activeInHierarchy && e.IsAlive)
            .ToArray();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius * DAMAGE_R_MULT); // 실제 피해 반경 표시
    }
}
