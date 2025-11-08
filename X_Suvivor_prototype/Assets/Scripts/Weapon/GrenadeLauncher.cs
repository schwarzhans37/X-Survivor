using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncher : WeaponBase
{
    [Header("유탄 발사기 설정")]
    [Tooltip("유탄이 폭발하기까지 날아갈 최대 거리")]
    public float travelDistance = 10f;

    protected override void Attack()
    {
        // 마우스 위치 또는 가장 가까운 적 방향으로 발사
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dir = (worldMousePos - transform.position).normalized;

        // currentCount 만큼 유탄 발사
        for (int i = 0; i < currentCount; i++)
        {
            Transform grenade = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).transform;
            grenade.position = transform.position;

            // 유탄의 이동 로직을 담당할 스크립트에 모든 데이터 전달
            grenade.GetComponent<Grenade>().Init(
                currentDamage, 
                currentRange,           // 폭발 반경 (기존과 동일)
                currentProjectileSpeed, // 유탄의 비행 속도 (기존과 동일)
                travelDistance,         // [신규] 유탄이 폭발하기까지 날아갈 거리
                dir                     // 발사 방향 (기존과 동일)
            );
        }
    }
}
