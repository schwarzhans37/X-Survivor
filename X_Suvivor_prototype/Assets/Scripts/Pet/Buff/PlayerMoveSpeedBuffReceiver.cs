using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class PlayerMoveSpeedBuffReceiver : MonoBehaviour
{
    private readonly List<float> _activeMultipliers = new List<float>();

    public float TotalMultiplier
    {
        get
        {
            if (_activeMultipliers.Count == 0) return 1f;
            float m = 1f;
            for (int i = 0; i < _activeMultipliers.Count; i++) m *= _activeMultipliers[i];
            return m;
        }
    }

    public void AddMultiplier(float multiplier, float duration)
    {
        StartCoroutine(Apply(multiplier, duration));
    }

    private IEnumerator Apply(float multiplier, float duration)
    {
        _activeMultipliers.Add(multiplier);
        // 이동 코드가 구독했다면 알려주기(선택)
        SendMessage("OnSpeedBuffChanged", TotalMultiplier, SendMessageOptions.DontRequireReceiver);

        yield return new WaitForSeconds(duration);

        _activeMultipliers.Remove(multiplier);
        SendMessage("OnSpeedBuffChanged", TotalMultiplier, SendMessageOptions.DontRequireReceiver);
    }
}
