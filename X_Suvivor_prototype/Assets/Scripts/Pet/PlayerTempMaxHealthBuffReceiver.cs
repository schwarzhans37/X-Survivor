using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class PlayerTempMaxHealthBuffReceiver : MonoBehaviour
{
    private class Entry { public int amount; public float endTime; }

    private readonly List<Entry> _entries = new List<Entry>();

    // 활성 엔트리 합
    public int TotalBonus
    {
        get
        {
            float now = Time.time;
            int sum = 0;
            for (int i = 0; i < _entries.Count; i++)
                if (_entries[i].endTime > now) sum += _entries[i].amount;
            return sum;
        }
    }

    /// <summary>임시 최대체력 증가. duration초 후 자동 만료.</summary>
    public void AddTemporary(int amount, float duration, bool healNewHeart)
    {
        if (amount <= 0 || duration <= 0f) return;

        var e = new Entry { amount = amount, endTime = Time.time + duration };
        _entries.Add(e);

        // 최대체력 변경 알림 & 즉시 회복 시도
        SendMessage("OnTempMaxHealthChanged", TotalBonus, SendMessageOptions.DontRequireReceiver);
        if (healNewHeart)
        {
            SendMessage("Heal", amount, SendMessageOptions.DontRequireReceiver);
            SendMessage("Heal", SendMessageOptions.DontRequireReceiver);
        }

        StartCoroutine(Expire(e));
    }

    private IEnumerator Expire(Entry e)
    {
        float wait = Mathf.Max(0f, e.endTime - Time.time);
        yield return new WaitForSeconds(wait);

        _entries.Remove(e);
        // 만료 시 최대체력 재계산 요청 & 초과 체력 클램프 요청
        SendMessage("OnTempMaxHealthChanged", TotalBonus, SendMessageOptions.DontRequireReceiver);
        SendMessage("ClampHealthToMax", SendMessageOptions.DontRequireReceiver);
        SendMessage("OnMaxHealthChanged", SendMessageOptions.DontRequireReceiver);
    }
}
