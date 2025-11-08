using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [Header("애니메이션 및 사운드")]
    [Tooltip("Animator에서 폭발을 실행시킬 Trigger 이름")]
    public string explodeTriggerName = "Explode";
    [Tooltip("폭발 애니메이션 클립의 실제 길이 (초)")]
    public float animationDuration = 0.6f;
    [Tooltip("폭발 오디오 클립")]
    public AudioClip explosionSfx;
    [Range(0f, 2f)] public float explosionSfxVolume = 1f;
    [Range(0.1f, 3f)] public float explosionSfxPitch = 1f;

    // --- 데이터 전달용 변수 ---
    private float damage;
    private float knockback;
    
    // --- 내부 참조 컴포넌트 ---
    private Animator animator;
    private CircleCollider2D circleCollider; // << [핵심] 자신의 콜라이더를 참조할 변수

    // 중복 폭발을 방지하는 플래그
    private bool hasExploded = false;

    void Awake()
    {
        // 자기 자신의 컴포넌트를 자동으로 찾아옵니다.
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    void OnEnable()
    {
        hasExploded = false;
    }

    // [수정] Init 함수에서 더 이상 radius를 받아오지 않습니다.
    public void Init(float dmg, float knock)
    {
        damage = dmg;
        knockback = knock;

        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (circleCollider == null)
        {
            Debug.LogError("ExplosionController에 CircleCollider2D가 없습니다!");
            return;
        }
        hasExploded = true;

        // 1. 사운드 재생
        if (explosionSfx != null)
        {
            AudioSource.PlayClipAtPoint(explosionSfx, transform.position, explosionSfxVolume);
        }

        // 2. 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger(explodeTriggerName);
        }

        // 3. 범위 내의 모든 적을 찾음
        LayerMask enemyMask = LayerMask.GetMask("Enemy");
        
        // [핵심 수정] 외부에서 받은 radius가 아닌, 자신의 circleCollider.radius 값을 사용합니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius, enemyMask);

        // 4. 찾은 모든 적에게 데미지 적용
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.ApplyDamage(damage, transform.position, knockback);
                Debug.Log($"{col.name}에게 {damage} 데미지 적용! (폭발 반경: {circleCollider.radius})");
            }
        }

        // 5. 지정된 시간 후에 비활성화
        StartCoroutine(DisableAfterAnimation());
    }

    private IEnumerator DisableAfterAnimation()
    {
        yield return new WaitForSeconds(animationDuration);
        gameObject.SetActive(false);
    }

    // 기즈모가 콜라이더 반경을 따라가도록 수정
    void OnDrawGizmosSelected()
    {
        // 에디터에서 콜라이더를 직접 참조하여 범위를 표시
        if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, circleCollider.radius);
    }
}