using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpBarUI : MonoBehaviour
{
    public Slider expSilder;

    void Awake()
    {
        expSilder = GetComponent<Slider>();
    }

    void LateUpdate()
    {
        float currentExp = GameManager.instance.nowExp;
        int level = GameManager.instance.level;
        int[] nextExpArray = GameManager.instance.needExp;

        float maxExp = nextExpArray[Mathf.Min(level, nextExpArray.Length - 1)];
        expSilder.value = currentExp / maxExp;
    }
}
