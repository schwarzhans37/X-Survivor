using UnityEngine;

// 업적을 구분할 탭 카테고리
public enum AchievementCategory { Overall, Combat, Spending }

// 보상 유형
public enum RewardType { PremiumCurrency, CharacterUnlock }

[CreateAssetMenu(fileName = "Achievement_", menuName = "Achievement/Achievement Data")]
public class AchievementData : ScriptableObject
{
    [Header("기본 정보")]
    public string id; // "total_kills_1", "gacha_spend_3" 등 고유 ID
    public AchievementCategory category; // 탭 구분을 위한 카테고리
    public string title;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("달성 조건")]
    [Tooltip("AchievementManager가 추적하는 통계 ID (예: totalKills, gachaSpent)")]
    public string statToTrack; 
    public long targetValue; // 달성 목표치 (누적값이므로 long 추천)

    [Header("보상")]
    public RewardType rewardType;
    public int rewardAmount; // 재화 보상량
    public int characterUnlockId; // 캐릭터 해금 시 ID (CharaData.charaId)

    [Header("연계 (선택 사항)")]
    [Tooltip("이 업적을 완료해야 다음 업적이 보임 (예: 100킬 -> 1000킬)")]
    public AchievementData nextAchievement; 
}