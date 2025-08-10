using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTextUI : MonoBehaviour
{
    public Text levelText;

    void Awake()
    {
        levelText = GetComponent<Text>();
    }

    void LateUpdate()
    {
        levelText.text = $"Lv.{GameManager.instance.level}";
    }
}
