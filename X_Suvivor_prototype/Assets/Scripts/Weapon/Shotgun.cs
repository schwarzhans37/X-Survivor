using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponBase
{
    private float currentSpreadAngle;   // 현재 레벨에 맞는 최종 발사 각도

    public override void Init(WeaponData data)
    {
        base.Init(data);
    }

    // 레벨업 시 발사 각도도 함께 갱신하도록 ApplyStatusByLevel 오버라이드
    public override void ApplyStatusByLevel()
    {
        base.ApplyStatusByLevel();      // 부모의 기본 스탯 적용 로직을 우선 실행

        // 레벨에 맞는 발사 각도 설정
        if (currentLevel == 0)
        {
            currentSpreadAngle = weaponData.baseSpreadAngle;
        }
        else
        {
            int levelIndex = currentLevel - 1;
            if (levelIndex < weaponData.spreadAngles.Length)
            {
                currentSpreadAngle = weaponData.spreadAngles[levelIndex];
            }
        }
    }

    protected override void Attack()
    {
        // 1. 마우스 방향으로 벡터 계산
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 centerDir = (worldMousePos - transform.position).normalized;

        // 2. currentCount 만큼 총알을 발사하는 루프
        for (int i = 0; i < currentCount; i++)
        {
            // 3. 오브젝트 풀에서 투사체를 가져옴
            Transform bulletTransform = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).transform;
            bulletTransform.position = transform.position;

            // 4. 총알의 발사 각도 계산
            // ex) 산탄총은 최소 3발부터 발사하도록 할 예정
            float angle;
            angle = -currentSpreadAngle / 2 + (currentSpreadAngle / (currentCount - 1)) * i;

            // 5. 계산된 각도만큼 중앙 벡터를 회전시켜서 최종 ㅂ아향 결정
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 finalDir = rotation * centerDir;

            // 6. 투사체 회전 및 초기화
            bulletTransform.rotation = Quaternion.FromToRotation(Vector3.up, finalDir);
            bulletTransform.GetComponent<Bullet>().Init(currentDamage, currentPenetration, currentProjectileSpeed, finalDir);
        }

        // 7. 사운드 재생
        AudioManager.instance.PlaySfx("Range");
    }
}
