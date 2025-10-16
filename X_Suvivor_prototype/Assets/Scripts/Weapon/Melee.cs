using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : WeaponBase
{
    private int meleePrefabId;  // 근접 무기 프리팹 ID

    public override void ApplyStatusByLevel()
    {
        base.ApplyStatusByLevel();
        Batch();
    }

    public override void Init(WeaponData data)
    {
        base.Init(data);    // 부모(WeaponBase)의 Init 로직을 먼저 실행

        // 손 등장 로직 (차후에는 삭제해야할 컨텐츠의 로직임)
        Hand hand = System.Array.Find(player.hands, h => h.isLeft);
        if (hand != null)
        {
            hand.spriter.sprite = data.handSprite;
            hand.gameObject.SetActive(true);
        }
    }

    protected override void Attack()
    {
        // 현재는 무기가 삽 뿐이므로, 삽을 회전시키는 로직만 존재
        // 차후 근접무기마다의 공격 패턴을 만들 계획
        transform.Rotate(Vector3.back * currentProjectileSpeed * Time.deltaTime);
    }

    public override void LevelUp()
    {
        base.LevelUp();
        // 레벨업 시 투사체 개수가 변하므로, Batch()를 다시 호출해 재배치함
        Batch();
    }

    private void Batch()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // 필요한 개수만큼 새로 배치
        for (int i = 0; i < currentCount; i++)
        {
            Transform blade = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).transform;
            blade.parent = transform; // 이 무기 오브젝트의 자식으로 설정

            blade.localPosition = Vector3.zero;
            blade.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * i / currentCount;
            blade.Rotate(rotVec);
            blade.Translate(blade.up * 1.5f, Space.World);
            
            blade.GetComponent<MeleeHitBox>().Init(currentDamage, -100);
        }
    }
}
