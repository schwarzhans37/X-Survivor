using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public Text timerText;

    void Awake()
    {
        timerText = GetComponent<Text>();
    }

    void LateUpdate()
    {
        float remainTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
        int min = Mathf.FloorToInt(remainTime / 60);
        int sec = Mathf.FloorToInt(remainTime % 60);
        timerText.text = $"{min:D2} : {sec:D2}";
    }
}
