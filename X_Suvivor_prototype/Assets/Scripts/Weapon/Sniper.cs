using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : WeaponBase
{
    // Sniper 클래스는 WeaponBase를 상속받아 Attack 로직만 재정의
    // 데미지, 쿨다운, 관통력 등 모든 핵심 데이터는 WeaponData 에셋을 통해 관리

    protected override void Attack()
    {
        // 1. 마우스의 스크린 좌표를 가져옴
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // 카메라와의 거리를 보정

        // 2. 스크린 좌표를 게임 월드 좌표로 변환
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // 3. 발사 방향 벡터를 계산 (플레이어 위치 -> 마우스 월드 위치)
        Vector2 dir = (worldMousePos - transform.position).normalized;

        // 4. 오브젝트 풀에서 저격총 탄환 프리팹을 가져옴
        // PoolManagerIndex는 이 무기의 WeaponData에 설정된 값을 사용
        Transform bulletTransform = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).transform;
        
        // 5. 탄환의 위치를 플레이어의 위치로 설정
        bulletTransform.position = transform.position;

        // 6. 탄환이 발사 방향을 바라보도록 회전 (Vector3.up이 기본 방향인 스프라이트 기준)
        bulletTransform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // 7. 탄환을 초기화, WeaponBase로부터 현재 레벨에 맞는 스탯들을 받아와 전달
        // 저격총의 특징(높은 데미지, 긴 사거리, 높은 관통력)은 WeaponData에 설정된 값에 따라 결정
        bulletTransform.GetComponent<Bullet>().Init(currentDamage, currentRange, currentPenetration, currentProjectileSpeed, dir);

        // 8. 저격총 격발 사운드를 재생
        AudioManager.instance.PlaySfx("Range");
    }
}
