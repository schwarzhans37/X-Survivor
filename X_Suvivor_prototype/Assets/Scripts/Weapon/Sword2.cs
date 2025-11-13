using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword2 : WeaponBase
{
    [Header("찌르기 설정")]
    [Tooltip("한 번 찌르기가 지속되는 시간")]
    public float thrustDuration = 0.15f;
    [Tooltip("찌르기 사이의 딜레이")]
    public float delayBetweenThrusts = 0.1f;

    private bool isAttacking = false;

    protected override void Attack()
    {
        if (isAttacking)
        {
            return;
        }

        StartCoroutine(ThrustCoroutine());
    }

    private IEnumerator ThrustCoroutine()
    {
        isAttacking = true;

        // 1. 마우스 방향 계산
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dir = (worldMousePos - transform.position).normalized;

        // 2. currentCount 만큼 찌르기 반복
        for (int i = 0; i < currentCount; i++)
        {
            // 3. 오브젝트 풀에서 '찌르기 이펙트/히트박스' 프리팹 가져오기
            GameObject thrustInstance = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, weaponData.PoolManagerIndex).gameObject;
            Transform thrustTransform = thrustInstance.transform;
            thrustTransform.parent = this.transform;

            // 4. 데미지 및 관통력 설정 (찌르기는 높은 관통력이 핵심)
            // MeleeHitBox의 Init 마지막 인자를 -100으로 주면 무한 관통처럼 사용 가능
            thrustInstance.GetComponent<MeleeHitBox>().Init(currentDamage, currentPenetration > 0 ? currentPenetration : -100);

            // 5. 찌르기 애니메이션 (앞으로 나아갔다 돌아오는 효과)
            float elapsedTime = 0f;
            Vector3 startPos = player.transform.position;
            // Z축 회전값을 dir 벡터에 맞게 설정
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            thrustTransform.rotation = Quaternion.Euler(0, 0, angle);

            while (elapsedTime < thrustDuration)
            {
                // 시간에 따라 앞으로 전진
                float progress = elapsedTime / thrustDuration;
                thrustTransform.position = Vector3.Lerp(startPos, startPos + (Vector3)dir * currentRange, progress);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 6. 공격이 끝나면 비활성화하여 풀로 반환
            thrustInstance.SetActive(false);

            // 7. 다음 찌르기 전 잠시 대기
            if (i < currentCount - 1)
            {
                yield return new WaitForSeconds(delayBetweenThrusts);
            }
        }

        isAttacking = false;
    }
}
