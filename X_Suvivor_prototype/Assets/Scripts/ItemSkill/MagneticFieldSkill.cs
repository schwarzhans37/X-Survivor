using System.Collections;
using UnityEngine;

public class MagneticFieldSkill : MonoBehaviour
{
    [Header("스킬 기본")]
    public float duration = 10f;          // 오라 유지 시간
    public float cooldown = 15f;         // 쿨타임
    public float cooldownTimer = 0f;
    public LayerMask enemyMask;          // Enemy 레이어

    [Header("범위/연출")]
    public float baseRadius = 3.5f;             // 원래 반경(설정값)
    public float visualScaleFactor = 1.1f;      // 오라 비주얼 크기
    public GameObject auraPrefab;               // 자기장 VFX 프리팹(루프 애니메이션)
    public int sortingOrderOffset = -1;         // 플레이어 스프라이트 기준 정렬 오프셋. -1=뒤, +1=앞

    [Header("도트/슬로우")]
    public float dps = 8f;               // 초당 피해량
    public float tickInterval = 0.5f;    // 틱 간격
    public float slowPercent = 0.5f;     // 0.5 = 50% 감속
    public float slowDuration = 0.6f;    // 슬로우 유지 시간(틱마다 갱신됨)

    [Header("SFX")]
    public AudioClip startSfx;          // 사용 즉시 1회 재생
    [Range(0.25f, 2f)] public float startSfxPitch = 1f;
    [Range(0f, 1.5f)] public float startSfxVolume = 1f;

    // 내부 상태
    bool active = false;

    // 참조
    SpriteRenderer playerSR;

    // 실제 판정 반경 (baseRadius * visualScaleFactor)
    float Radius => baseRadius * visualScaleFactor;

    void Awake()
    {
        playerSR = GetComponent<SpriteRenderer>(); // 정렬 보정용(선택)
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public bool TryUse()
    {
        if (active) return false;
        if (cooldownTimer > 0f) return false;
        if (auraPrefab == null) { Debug.LogWarning("[MagneticField] auraPrefab이 비었습니다."); return false; }

        StartCoroutine(CoRun());
        return true;
    }

    public float GetCooldownRatio()
    {
        return Mathf.Clamp01(cooldownTimer / cooldown);
    }

    IEnumerator CoRun()
    {
        active = true;
        cooldownTimer = cooldown;

        // ▶ 사용 순간 효과음
        if (startSfx && AudioManager.instance)
            AudioManager.instance.PlaySfx(startSfx, startSfxPitch, startSfxVolume);

        // 오라 생성
        var aura = Instantiate(auraPrefab, transform.position, Quaternion.identity, transform);
        aura.transform.localPosition = Vector3.zero;

        // 스프라이트 크기 읽어서 radius 반경에 맞게 스케일
        float scale = 1f;
        var auraSR = aura.GetComponentInChildren<SpriteRenderer>();
        if (auraSR != null && auraSR.sprite != null)
        {
            float spriteUnitWidth = auraSR.sprite.bounds.size.x; // 1배 스케일에서의 너비
            if (spriteUnitWidth > 0f)
                scale = (Radius * 2f / spriteUnitWidth); // 반경*2(지름) / 원본너비
        }
        aura.transform.localScale = new Vector3(scale, scale, 1f);

        // 정렬
        if (auraSR != null && playerSR != null)
        {
            auraSR.sortingLayerID = playerSR.sortingLayerID;
            auraSR.sortingOrder = playerSR.sortingOrder + sortingOrderOffset;
        }

        // DOT + 슬로우 루프
        float t = 0f, acc = 0f;
        while (t < duration)
        {
            t += Time.deltaTime; acc += Time.deltaTime;
            if (acc >= tickInterval)
            {
                ApplyTick(dps * acc);
                acc = 0f;
            }
            yield return null;
        }
        if (acc > 0f) ApplyTick(dps * acc);

        if (aura != null) Destroy(aura);
        active = false;
    }

    void ApplyTick(float damageThisTick)
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, Radius, enemyMask);
        foreach (var c in cols)
        {
            if (c.TryGetComponent<Enemy>(out var e))
            {
                e.ApplyDamage(damageThisTick, transform.position, 0f);
                e.ApplySlow(slowPercent, slowDuration);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.7f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
