using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : MonoBehaviour
{
    [Header("탐지/폭발")]
    public LayerMask enemyMask;         // OverlapCircle/All이 탐지할 레이어(반드시 Enemy 레이어 체크!)
    public float triggerRadius = 0.5f;  // '밟음' 판정 범위(작게): 이 반경 안에 적이 들어오면 폭발
    public float explosionRadius = 1.6f;// 실제 데미지가 들어가는 폭발 범위(크게)
    public int damage = 30;             // 폭발 데미지(예: 30, 40 등)
    public float knockback = 5f;        // 폭발 중심에서 바깥 방향으로 밀어내는 힘

    [Header("비주얼")]
    public SpriteRenderer spriteRenderer;   // 지뢰 본체 스프라이트
    public Animator animator;               // 폭발 애니메이터 (Bomb_Effect)
    public string explodeTrigger = "Explode";
    public GameObject effectPrefab;         // Bomb_Effect를 프리팹으로 뽑았다면 사용(선택)
    public float effectLifetime = 1.0f;          // 프리팹 사용 시 이펙트 생존 시간
    public float explodeAnimDuration = 0.6f;     // 자기 Animator 사용 시 폭발 클립 길이

    [Header("기타")]
    public bool destroyAfterExplode = true; // 폭발 후 이 오브젝트를 파괴할지(false면 SetActive(false))

    private bool armed = false;             // true가 되기 전에는 감지/폭발 안 함(자기 발동 방지)
    private bool exploding = false;         // 중복 폭발 방지 플래그

    public System.Action OnExploded;        // 외부에서 '터졌다'를 알 수 있도록 이벤트(리스트 관리 등)

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
        if (!armed || exploding) return;

        // 원 안에 적이 들어오면 폭발
        Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, enemyMask);
        if (hit != null)
        {
            Explode();                          // 적이 밟으면 폭발
        }
    }

    //강제 폭발 (현재는 작동하는 로직 없음)
    public void ForceExplode(bool applyDamage = true)
    {
        if (exploding) return;
        Explode(applyDamage);
    }

    private void Explode(bool applyDamage = true)
    {
        if (exploding) return;
        exploding = true;

        // 1) 비주얼 처리
        bool deferFinish = false; // 애니 끝까지 보여준 뒤 정리할지 여부

        if (effectPrefab != null)
        {
            // 프리팹 이펙트 생성(본체 정리와 독립)
            var fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            // 가시성 보정: 지뢰와 같은 레이어/오더 + 1
            var srFx = fx.GetComponentInChildren<SpriteRenderer>();
            if (srFx != null && spriteRenderer != null)
            {
                srFx.sortingLayerID = spriteRenderer.sortingLayerID;
                srFx.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            Destroy(fx, effectLifetime);
        }
        else if (animator != null)
        {
            // 자기 Animator로 트리거 → 끝까지 보여주고 정리
            animator.ResetTrigger(explodeTrigger);
            animator.SetTrigger(explodeTrigger);
            deferFinish = true;
            StartCoroutine(CoFinishAfter(explodeAnimDuration));
        }

        // 데미지/넉백
        if (applyDamage)
        {
            // 폭발 반경 안의 Enemy 레이어 콜라이더 수집
            var cols = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyMask);
            foreach (var c in cols)
            {
                // Enemy 컴포넌트가 있다면 공용 데미지 API 호출
                // (Enemy.ApplyDamage는 체력 감소 + Hit 연출 + 사망 처리 + 넉백까지 담당)
                if (c.TryGetComponent<Enemy>(out var enemy))
                {
                    // LandMine.damage(int) -> float 캐스팅
                    enemy.ApplyDamage((float)damage, transform.position, knockback);
                }
            }
        }


        OnExploded?.Invoke();

        if (!deferFinish) Finish();
    }

    private IEnumerator CoFinishAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Finish();
    }

    private void Finish()
    {
        if (destroyAfterExplode) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        // 폭발 범위(빨간색)
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        // 밟힘 범위(초록색)
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
