using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillCountUI : MonoBehaviour
{
    public Text killText;

    void Awake()
    {
        killText = GetComponent<Text>();
    }

    void LateUpdate()
    {
        killText.text = $"{GameManager.instance.killCount}";
    }
}
