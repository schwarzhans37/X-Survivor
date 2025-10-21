using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : WeaponBase
{
    [Header("# 투사체 프리팹 ID")]
    private int projectileId;

    public override void Init(WeaponData data)
    {
        base.Init(data);    // 부모 클래스의 Init 실행

        /* 손 등장 로직 (이제는 사용하지 않는 로직, 차후 삭제 필요)
        Hand hand = System.Array.Find(player.hands, h => !h.isLeft);
        if (hand != null)
        {
            hand.spriter.sprite = data.handSprite;
            hand.gameObject.SetActive(true);
        }*/
    }

    protected override void Attack()
    {
        // 1. 마우스의 스크린 좌표를 가져옴
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;

        // 2. 스크린 좌표를 월드 좌표로 변환
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // 3. 월드 좌표에서 방향 벡터 계산
        Vector2 dir = (worldMousePos - transform.position).normalized;

        // 4. 오브젝트 풀에서 투사체를 가져옴
        Transform bulletTransform = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).transform;
        bulletTransform.position = transform.position;
        bulletTransform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // 5. 투사체를 초기화함.
        bulletTransform.GetComponent<Bullet>().Init(currentDamage, currentRange, currentPenetration, currentProjectileSpeed, dir);

        // 6. 사운드 재생
        AudioManager.instance.PlaySfx("Range");
    }
}