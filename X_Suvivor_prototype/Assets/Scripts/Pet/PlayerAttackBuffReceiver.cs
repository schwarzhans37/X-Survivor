using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어가 일시적으로 공격력(플랫)을 얻는 버프 관리자.
/// 기존 데미지 계산에 더하기만 하면 된다:
///   최종공격력 = 기본공격력 + PlayerAttackBuffReceiver.TotalFlatBonus
/// </summary>
[DisallowMultipleComponent]
public class PlayerAttackBuffReceiver : MonoBehaviour
{
    private readonly List<float> _activeBonuses = new List<float>();

    /// <summary>현재 활성화된 모든 플랫 보너스 합</summary>
    public float TotalFlatBonus
    {
        get
        {
            float sum = 0f;
            for (int i = 0; i < _activeBonuses.Count; i++) sum += _activeBonuses[i];
            return sum;
        }
    }

    public void AddFlatAttackBuff(float amount, float duration)
    {
        StartCoroutine(ApplyFlatBuff(amount, duration));
    }

    private IEnumerator ApplyFlatBuff(float amount, float duration)
    {
        _activeBonuses.Add(amount);
        yield return new WaitForSeconds(duration);
        _activeBonuses.Remove(amount);
    }
}
