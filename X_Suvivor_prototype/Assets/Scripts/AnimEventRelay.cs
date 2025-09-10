using UnityEngine;

public class AnimEventRelay : MonoBehaviour
{
    Enemy enemy;

    void Awake()
    {
        // 한 번만 찾아 캐싱(매 프레임 GetComponent 호출 방지)
        enemy = GetComponentInParent<Enemy>();
    }

    // Death 애니메이션 끝에서 호출
    public void Dead()
    {
        enemy?.Dead();
    }

    // Attack 애니메이션 중 "켜는" 프레임에서 호출
    public void AE_EnableHitbox()
    {
        enemy?.AE_EnableHitbox();
    }

    // Attack 애니메이션 중 "끄는" 프레임에서 호출
    public void AE_DisableHitbox()
    {
        enemy?.AE_DisableHitbox();
    }

    // 원거리 공격 애니메이션에서 투사체를 발사하는 프레임에 호출합니다.
    public void AE_FireProjectile()
    {
        enemy?.AE_FireProjectile();
    }
}
