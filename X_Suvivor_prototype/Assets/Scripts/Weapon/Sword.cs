using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : WeaponBase
{
    [Header("검 휘두르기 설정")]
    [Tooltip("검을 한 번 휘두르는 데 걸리는 시간")]
    public float swingDuration = 0.3f;
    [Tooltip("검이 플레이어로부터 떨어져서 생성되는 거리")]
    public float swingDistance = 1.5f;

    // 공격이 이미 진행 중인지 확인하여 중복 실행 방지
    private bool isSwinging = false;

    // Attack()이 호출되면 Swing 코루틴을 시작
    protected override void Attack()
    {
        if (isSwinging)
        {
            return;
        }

        StartCoroutine(SwingCoroutine());
    }

    private IEnumerator SwingCoroutine()
    {
        isSwinging = true;

        // 1. 마우스 방향 계산
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dir = (worldMousePos - transform.position).normalized;

        // 2. 오브젝트 풀에서 '검' 프리팹을 가져와 활성화
        GameObject swordInstance = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).gameObject;
        Transform swordTransform = swordInstance.transform;
        swordTransform.parent = this.transform;     // 컨트롤러의 자식으로 설정

        // 3. 데미지, 관통력 등 초기화
        swordInstance.GetComponent<Bullet>().Init(currentDamage, -100, 0, Vector2.zero);

        // 4. 스윙 애니메이션
        float elapsedTime = 0f;

        // 휘두르기 시작 각도 : 마우스 방향의 왼쪽 90도
        Quaternion startRotation = Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0, 0, 90);
        // 휘두르기 끝 각도 : 마우스 방향의 오른쪽 90도
        Quaternion endRotation = Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0, 0, -90);

        while (elapsedTime < swingDuration)
        {
            // 시간에 따라 시작 각도에서 끝 각도로 회전
            float progress = elapsedTime / swingDuration;
            swordTransform.rotation = Quaternion.Slerp(startRotation, endRotation, progress);

            // 플레이어 주변에서 회전하도록 위치 설정
            swordTransform.position = player.transform.position + swordTransform.up * swingDistance;

            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 5. 스윙이 끝나면 거을 비활성화하여 풀로 반환
        swordInstance.SetActive(false);
        isSwinging = false;
    }
}
