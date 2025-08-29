using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SplitArrowsSkill : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject arrowPrefab;     // ▶ 화살 프리팹(아래 PiercingArrow.cs 포함)
    public int arrowCount = 12;        // 12방향
    public float arrowSpeed = 12f;     // 화살 속도(빠르게)
    public int damage = 20;            // 1회 타격 데미지
    public float duration = 6f;        // 전체 지속 시간(6초)
    public float cooldown = 10f;       // 쿨타임
    public LayerMask enemyMask;        // Enemy 레이어

    float cooldownTimer;
    bool casting;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    // Input Actions에서 2번키에 바인딩한 액션이 호출
    public void OnSplitArrow(InputValue v)
    {
        TryUse();
    }

    public bool TryUse()
    {
        if (casting) return false;
        if (cooldownTimer > 0f) return false;
        if (arrowPrefab == null) { Debug.LogWarning("[PiercingArrows] arrowPrefab이 비었습니다."); return false; }

        StartCoroutine(CoCast());
        return true;
    }

    IEnumerator CoCast()
    {
        casting = true;
        cooldownTimer = cooldown;

        // 360도를 arrowCount로 균등 분할하여 발사
        Vector3 origin = transform.position;
        float step = 360f / Mathf.Max(1, arrowCount);

        for (int i = 0; i < arrowCount; i++)
        {
            float ang = step * i;                 // 도 단위
            Vector2 dir = AngleToDir(ang);        // 단위 방향
            SpawnArrow(origin, dir);
        }

        // 전체 지속 시간 후 종료(화살 자체도 내부 lifeTime으로 사라짐)
        yield return new WaitForSeconds(duration);

        casting = false;
    }

    void SpawnArrow(Vector3 pos, Vector2 dir)
    {
        var go = Instantiate(arrowPrefab, pos, Quaternion.identity);
        var arrow = go.GetComponent<PiercingArrow>();
        if (arrow != null)
        {
            arrow.Init(dir * arrowSpeed, damage, duration, enemyMask);
        }
        else
        {
            Debug.LogWarning("[PiercingArrows] 프리팹에 PiercingArrow 컴포넌트가 없습니다.");
        }
    }

    static Vector2 AngleToDir(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }
}


