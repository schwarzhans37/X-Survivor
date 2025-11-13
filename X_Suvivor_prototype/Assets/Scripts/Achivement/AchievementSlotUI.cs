using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementSlotUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Image iconImage;
    public Text titleText;
    public Text descriptionText;
    public Text progressText;
    public Button claimButton;
    public Text claimButtonText;

    private AchievementData data;

    // 업적 슬롯을 데이터로 초기화
    public void Setup(AchievementData achievementData)
    {
        this.data = achievementData;

        // 1. 기본 정보 설정
        iconImage.sprite = data.icon;
        titleText.text = data.title;
        descriptionText.text = data.description;

        // 2. 진행도 및 버튼 상태 갱신
        Refresh();
    }

    // 상태 갱신 (보상 수령 후 호출될 수 있음)
    public void Refresh()
    {
        if (data == null) return;

        AchievementManager.AchievementStatus status =
            AchievementManager.instance.GetAchievementStatus(data);

        long currentValue = AchievementManager.instance.GetStatValue(data.statToTrack);
        long targetValue = data.targetValue;

        switch (status)
        {
            case AchievementManager.AchievementStatus.InProgress:
                progressText.text = $"{currentValue} / {targetValue}";
                claimButton.interactable = false;
                claimButtonText.text = "미달성";
                break;
            case AchievementManager.AchievementStatus.Completed:
                progressText.text = $"완료 ({targetValue} / {targetValue})";
                claimButton.interactable = true;
                claimButtonText.text = "보상 받기";
                break;
            case AchievementManager.AchievementStatus.Claimed:
                progressText.text = "달성 완료";
                claimButton.interactable = false;
                claimButtonText.text = "수령 완료";
                break;
        }

        // 버튼 클릭 이벤트 설정
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClaimButtonClicked);
    }

    void OnClaimButtonClicked()
    {
        // 매니저에게 보상 요청
        AchievementManager.instance.ClaimReward(data.id);

        // 버튼 클릭 후 즉시 UI 갱신
        Refresh();
    }
}
