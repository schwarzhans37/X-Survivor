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

        // 투사체 프리팹 ID 찾기
        for (int i = 0; i < GameManager.instance.pool.prefabs.Length; i++)
        {
            if (data.projectile == GameManager.instance.pool.prefabs[i])
            {
                projectileId = i;
                break;
            }
        }

        // 무기 장비 로직(플레이어 캐릭터 스프라이트 위에 무기 스프라이트를 그려냄)
        Hand hand = player.hands[(int)data.weaponType];
        hand.spriter.sprite = data.handSprite;
        hand.gameObject.SetActive(true);
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
        Transform bulletTransform = GameManager.instance.pool.Get(projectileId).transform;
        bulletTransform.position = transform.position;
        bulletTransform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // 5. 투사체를 초기화함.
        bulletTransform.GetComponent<Bullet>().Init(currentDamage, currentPenetration, currentProjectileSpeed, dir);

        // 6. 사운드 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}