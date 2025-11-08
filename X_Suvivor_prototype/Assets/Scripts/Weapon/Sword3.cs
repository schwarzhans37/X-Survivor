using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword3 : WeaponBase
{
    [Header("검무 설정")]
    [Tooltip("검무가 활성화되어 있는 지속 시간")]
    public float activeDuration = 5f;
    [Tooltip("검무의 공전 반경")]
    public float orbitDistance = 2.0f; // 부유검(3f)보다 가까운 거리로 설정

    private float activeTimer = 0f;     // 검무가 활성화된 시간을 측정하는 타이머
    private bool isDanceActive = false; // 검무가 현재 활성화 상태인지 여부

    // 레벨업 시 무기를 재배치하기 위해 오버라이드
    public override void LevelUp()
    {
        base.LevelUp();
        // 레벨업으로 currentCount가 변경되면 즉시 재배치
        if (isDanceActive)
        {
            Batch();
        }
    }

    // WeaponBase의 Update를 오버라이드하여 새로운 로직으로 대체
    protected override void Update()
    {
        if (!GameManager.instance.isLive) return;

        // 1. 검무가 비활성화 상태일 때: 쿨다운을 계산하여 활성화
        if (!isDanceActive)
        {
            timer += Time.deltaTime;
            if (timer >= currentCooldown)
            {
                timer = 0f;
                ActivateSwordDance();
            }
        }
        // 2. 검무가 활성화 상태일 때: 지속시간을 계산하고, 회전
        else
        {
            activeTimer += Time.deltaTime;

            // 지속시간이 끝나면 비활성화
            if (activeTimer >= activeDuration)
            {
                DeactivateSwordDance();
            }

            // 부유검과 동일한 회전 로직
            transform.Rotate(Vector3.back * currentProjectileSpeed * Time.deltaTime);
        }
    }

    // 검무 활성화: 검들을 배치하고 타이머를 리셋
    private void ActivateSwordDance()
    {
        isDanceActive = true;
        activeTimer = 0f;
        Batch();
    }

    // 검무 비활성화: 배치된 모든 검을 풀로 반환
    private void DeactivateSwordDance()
    {
        isDanceActive = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    // 검들을 배치하는 로직 (Sword4.cs의 Batch와 거의 동일)
    private void Batch()
    {
        // 기존에 있던 자식 오브젝트(검)들을 모두 풀로 반환
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // 현재 레벨의 currentCount 만큼 새로 배치
        for (int i = 0; i < currentCount; i++)
        {
            Transform blade = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).transform;
            blade.parent = transform; // 이 컨트롤러의 자식으로 설정

            blade.localPosition = Vector3.zero;
            blade.localRotation = Quaternion.identity;

            // 360도를 검의 개수만큼 나누어 각도를 계산
            Vector3 rotVec = Vector3.forward * 360 * i / currentCount;
            blade.Rotate(rotVec);
            
            // 설정된 공전 거리(orbitDistance)만큼 플레이어로부터 떨어트림
            blade.Translate(blade.up * orbitDistance, Space.World);
            
            blade.GetComponent<MeleeHitBox>().Init(currentDamage, -100); // 무한 관통
        }
    }

    // 이 무기는 Attack() 메소드를 사용하지 않으므로 비워둡니다.
    protected override void Attack()
    {
        // Update 로직에서 모든 것을 처리하므로 이 메소드는 사용되지 않습니다.
    }
}
