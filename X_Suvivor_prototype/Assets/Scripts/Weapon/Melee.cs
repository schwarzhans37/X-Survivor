using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : WeaponBase
{
    private int meleePrefabId;  // 근접 무기 프리팹 ID

    public override void Init(WeaponData data)
    {
        base.Init(data);    // 부모(WeaponBase)의 Init 로직을 먼저 실행

        // 근접 무기 투사체 프리팹 ID값 찾기
        for (int i = 0; i < GameManager.instance.pool.prefabs.Length; i++)
        {
            if (weaponData.projectilePrefab == GameManager.instance.pool.prefabs[i])
            {
                meleePrefabId = i;
                break;
            }
        }

        Batch();    // 투사체 배치

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
        transform.Rotate(Vector3.back * 500f * Time.deltaTime);
    }

    public override void LevelUp()
    {
        base.LevelUp();
        // 레벨업 시 투사체 개수가 변하므로, Batch()를 다시 호출해 재배치함
        Batch();
    }

    private void Batch()
    {
        for (int i = 0; i < currentCount; i++)
        {
            Transform blade;    // 근접무기 프리팹 한개를 가리킬 변수

            // 이미 생성한 투사체가 있다면 재사용, 부족하면 새로 생성
            if (i < transform.childCount)
            {
                blade = transform.GetChild(i);
            }
            else
            {
                blade = GameManager.instance.pool.Get(meleePrefabId).transform;
                blade.parent = transform;
            }

            // 모든 투사체의 위치와 회전을 초기화
            blade.localPosition = Vector3.zero;
            blade.localRotation = Quaternion.identity;

            // 정해진 각도만큼 회전시킴
            Vector3 rotVec = Vector3.forward * 360 * i / currentCount;
            blade.Rotate(rotVec);

            // 캐릭터를 중심으로 일정 거리만큼 이동시켜 원형으로 배치
            blade.Translate(blade.up * 1.5f, Space.World);

            // 근접 무기용 투사체 초기화
            blade.GetComponent<Bullet>().Init(currentDamage, -100, 0, Vector2.zero);
        }

        // 레벨업으로 투사체 수가 줄어들 경우를 대비해, 남는 투사체는 비활성화
        for (int i = currentCount; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
