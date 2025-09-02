using UnityEngine;

public class AnimEventRelay : MonoBehaviour
{
    // AnimationEvent에서 호출됨
    public void Dead()
    {
        // 부모 Enemy 컴포넌트 찾아서 Dead() 실행
        GetComponentInParent<Enemy>()?.Dead();
    }
}
