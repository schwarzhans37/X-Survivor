using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : MonoBehaviour
{
    [Header("탐지/폭발")]
    public LayerMask enemyMask;                 // OverlapCircle/All이 탐지할 레이어(반드시 Enemy 레이어 체크!)
    public float triggerRadius = 0.5f;          // '밟음' 판정 범위(작게): 이 반경 안에 적이 들어오면 폭발
    public float baseExplosionRadius = 1.6f;    // 기본 폭발 반경(지름=반경*2)
    public float explosionScaleFactor = 1.5f;   // 폭발 크기 배수
    public int damage = 40;                     // 폭발 데미지(예: 30, 40 등)
    public float knockback = 5f;                // 폭발 중심에서 바깥 방향으로 밀어내는 힘
    public float explodeDelay = 1f;             // 밟은 후 1초 뒤 폭발

    [Header("비주얼")]
    public SpriteRenderer spriteRenderer;   // 지뢰 본체 스프라이트
    public Animator animator;               // 폭발 애니메이터 (Bomb_Effect)
    public string explodeTrigger = "Explode";
    public float effectLifetime = 1.0f;          // 프리팹 사용 시 이펙트 생존 시간
    public float explodeAnimDuration = 0.6f;     // 자기 Animator 사용 시 폭발 클립 길이

    [Header("기타")]
    public bool destroyAfterExplode = true; // 폭발 후 이 오브젝트를 파괴할지(false면 SetActive(false))

    private bool armed = false;             // true가 되기 전에는 감지/폭발 안 함(자기 발동 방지)
    private bool exploding = false;         // 중복 폭발 방지 플래그
    bool scheduled = false;                 // 지연 폭발 예약 여부(중복 예약 방지)

    public System.Action OnExploded;        // 외부에서 '터졌다'를 알 수 있도록 이벤트(리스트 관리 등)

    // 폭발 실제 반경(비주얼용)
    float ExplosionRadius => baseExplosionRadius * explosionScaleFactor;
    // 데미지 판정 반경 (200%정도 반경 늘려야 애니메이션과 범위 비슷함)
    float DamageRadius => ExplosionRadius * 2f;

    void Awake()
    {
        // 슬롯 비었으면 자동으로 찾아오기(편의)
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Arm(float delay)
    {
        if (delay <= 0f) { armed = true; return; }  // 설치 직후 바로 터지지 않도록, delay 후에 armed = true로 만든다.
        StartCoroutine(CoArm(delay));
    }

    IEnumerator CoArm(float delay)
    {
        armed = false;                          // 지연 동안 비무장 상태
        yield return new WaitForSeconds(delay);
        armed = true;                           // 지연 끝나면 감지/폭발 가능
    }

    void Update()
    {
        // 아직 무장 안됐거나 이미 폭발 진행 중이면 아무 것도 안 함
        if (!armed || exploding || scheduled) return;

        // 원 안에 적이 들어오면 폭발
        Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, enemyMask);
        if (hit != null)
        {
            scheduled = true;
            StartCoroutine(CoExplodeAfterDelay(explodeDelay));
        }
    }

    IEnumerator CoExplodeAfterDelay(float delay)
    {
        float t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Explode(true);
    }
    void Explode(bool applyDamage = true)
    {
        if (exploding) return;
        exploding = true;

        // ===== 폭발 비주얼: Animator 복제 후 폭발 반경에 정확히 맞는 스케일 =====
        if (animator != null)
        {
            var fx = Instantiate(animator.gameObject, transform.position, Quaternion.identity);
            var fxSR = fx.GetComponentInChildren<SpriteRenderer>();
            var fxAni = fx.GetComponent<Animator>();

            // 스프라이트 너비를 읽어 '지름 = ExplosionRadius * 2'에 딱 맞게 스케일
            if (fxSR != null && fxSR.sprite != null)
            {
                float spriteUnitWidth = fxSR.sprite.bounds.size.x; // 스케일 1에서의 너비(월드 유닛)
                if (spriteUnitWidth > 0f)
                {
                    float scale = (ExplosionRadius * 2f) / spriteUnitWidth;
                    fx.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }

            if (fxAni != null)
            {
                fxAni.ResetTrigger(explodeTrigger);
                fxAni.SetTrigger(explodeTrigger);
            }

            Destroy(fx, Mathf.Max(0.01f, explodeAnimDuration));
        }
        else
        {
            Debug.LogWarning("[LandMine] Animator가 비어있습니다. 폭발 비주얼이 출력되지 않습니다.");
        }

        // ===== 데미지/넉백 =====
        if (applyDamage)
        {
            var cols = Physics2D.OverlapCircleAll(transform.position, DamageRadius, enemyMask);
            foreach (var c in cols)
            {
                if (c.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.ApplyDamage(damage, transform.position, knockback);
                }
            }
        }

        OnExploded?.Invoke();

        // ===== 정리 =====
        if (destroyAfterExplode) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        // 폭발 비주얼 반경(빨간색)
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);

        // 실제 데미지 판정 반경(노란색)
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, DamageRadius);

        // 밟힘 판정 반경(초록색)
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}