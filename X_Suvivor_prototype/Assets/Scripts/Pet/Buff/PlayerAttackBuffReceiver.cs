using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class PlayerAttackBuffReceiver : MonoBehaviour
{
    private readonly List<float> _activeMultipliers = new List<float>();

    public float TotalMultiplier
    {
        get
        {
            if (_activeMultipliers.Count == 0)
                return 1f; // 버프가 없으면 1배 (곱셈에 영향 없음)

            float total = 1f;
            foreach (float multiplier in _activeMultipliers)
            {
                total *= multiplier;
            }
            return total;
        }
    }

    public void AddMultiplier(float multiplier, float duration)
    {
        StartCoroutine(ApplyMultiplierBuff(multiplier, duration));
    }

    private IEnumerator ApplyMultiplierBuff(float multiplier, float duration)
    {
        _activeMultipliers.Add(multiplier);
        yield return new WaitForSeconds(duration);
        _activeMultipliers.Remove(multiplier);
    }
}