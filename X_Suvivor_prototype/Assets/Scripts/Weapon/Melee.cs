using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : WeaponBase
{
    private int proejectileId;  // 근접 무기의 투사체(히트박스 오브젝트) ID
    
    public override void Init(WeaponData data)
    {
        base.Init(data);

        // 근접 무기 투사체 프리팹 ID값 찾기
        for (int i = 0; i < GameManager.instance.pool.prefabs.Length; i++)
        {
            if (weaponData.projectile == GameManager.instance.pool.prefabs[i])
            {
                projectileId = i;
                break;
            }
        }

        Batch();    // 투사체 배치
    }

    public override void LevelUp()
    {
        base.LevelUp();
        // 레벨업 시 투사체 개수가 변하므로, Batch()를 다시 호출해 재배치함
        Batch();
    }

    protected override void Attack()
    {
        transform.Rotate(Vector3.back * 500f * Time.deltaTime);
    }

    private void Batch()
    {
        for (int i = 0; i < currentCount; i++)
        {
            Transform projectile;

            // 이미 생성한 투사체가 있다면 재사용, 부족하면 새로 생성
            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(projectilePrefabId).transform;
                bullet.parent = transform;
            }

            // 모든 투사체의 위치와 회전을 초기화합니다.
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            
            // 투사체를 원형으로 배치합니다.
            Vector3 rotVec = Vector3.forward * 360 * i / currentCount;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);

            // 근접 무기용 투사체 초기화
            bullet.GetComponent<Bullet>().Init(currentDamage, -100, 0, Vector2.zero);
        }

        // 레벨업으로 투사체 수가 줄어들 경우를 대비해, 남는 투사체는 비활성화
        for (int i = currentCount; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
    }
}
