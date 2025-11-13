using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("UI 컨트롤러 참조")]
    [Tooltip("GamePlay씬의 HUD 오브젝트 하 관리되는 그룹 전체 관리 및 하위 UI 컨트롤러들에 대한 참조를 제공")]
    public HealthUI healthUI;
    public ExpBarUI expBarUI;
    public LevelTextUI levelTextUI;
    public KillCountUI killCountUI;
    public TimerUI timerUI;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
