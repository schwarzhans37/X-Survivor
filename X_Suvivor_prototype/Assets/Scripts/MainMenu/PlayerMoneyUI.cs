using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMoneyUI : MonoBehaviour
{
    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemsText;

    // UI가 활성화될 때마다 최신 정보로 업데이트하기 위해 OnEnable을 사용
    void OnEnable()
    {
        // PlayerDataManager의 재화 변경 이벤트를 '구독'함
        // OnPlayerDataUpdated 이벤트가 발생하면, 내 UpdateUITexts 함수를 실행
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerDataUpdated += UpdateUITexts;
        }

        // 활성화되는 시점에서도 한번 최신 정보로 업데이트
        UpdateUITexts();
    }

    void OnDisable()
    {
        // '구독을 취소' => 메모리 누수 방지
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerDataUpdated -= UpdateUITexts;
        }
    }

    // 실제 UI 텍스트를 업데이트하는 함수
    private void UpdateUITexts()
    {
        if (PlayerDataManager.instance == null) return;

        // PlayerDataManager로부터 최신 재화 정보를 가져와 UI에 표시
        goldText.text = PlayerDataManager.instance.playerData.gold.ToString("N0"); // 1,000 처럼 쉼표 추가
        gemsText.text = PlayerDataManager.instance.playerData.gems.ToString("N0");
    }
}