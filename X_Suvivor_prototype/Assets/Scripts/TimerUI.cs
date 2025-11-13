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

    // Update is called once per frame
    void LateUpdate()
    {
        // GameManager에서 직접 경과 시간(gameTime)을 가져옵니다.
        float elapsedTime = GameManager.instance.gameTime;
        
        // 경과 시간을 분과 초로 변환합니다.
        int min = Mathf.FloorToInt(elapsedTime / 60);
        int sec = Mathf.FloorToInt(elapsedTime % 60);
        
        // UI 텍스트를 "분:초" 형태로 업데이트합니다.
        timerText.text = $"{min:D2} : {sec:D2}";
    }
}
