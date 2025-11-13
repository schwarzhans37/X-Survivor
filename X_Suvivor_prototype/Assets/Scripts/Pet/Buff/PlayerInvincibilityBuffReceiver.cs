using UnityEngine;
using System.Collections;

// 플레이어에게 일시적인 무적 효과를 부여하는 스크립트
[DisallowMultipleComponent]
public class PlayerInvincibilityBuffReceiver : MonoBehaviour
{
    private int invincibilityRequestCount = 0;

    // 외부에서 무적 상태인지 확인할 수 있는 속성(Property)
    public bool IsInvincible => invincibilityRequestCount > 0;

    /// <summary>
    /// 지정된 시간(duration) 동안 플레이어를 무적으로 만듭니다.
    /// </summary>
    public void GrantInvincibility(float duration)
    {
        StartCoroutine(InvincibilityRoutine(duration));
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        invincibilityRequestCount++;
        // TODO: 여기에 플레이어의 레이어를 'Invincible' 같은 것으로 잠시 바꾸거나,
        // 플레이어의 피격 판정 로직에서 IsInvincible 값을 체크하도록 로직을 추가해야 합니다.
        Debug.Log($"무적 효과 시작! {duration}초 동안 지속됩니다.");

        yield return new WaitForSeconds(duration);

        invincibilityRequestCount--;
        Debug.Log("무적 효과 종료!");
    }
}