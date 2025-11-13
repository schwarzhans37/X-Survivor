using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    [Header("업적 데이터베이스")]
    [Tooltip("프로젝트 내의 모든 AchievementData SO를 담는 리스트")]
    public List<AchievementData> allAchievements;

    private AchievementSaveData saveData;
    private string saveKey = "AchievementData"; // PlayerPrefs에 저장할 키

    // UI에 알림을 보내기 위한 이벤트
    public static event System.Action<AchievementData> OnAchievementCompleted;  // 업적 달성 시
    public static event System.Action OnAchievementClaimed;     // 보상 수령 시

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadData()
    {
        string json = PlayerPrefs.GetString(saveKey, "{}");
        saveData = JsonUtility.FromJson<AchievementSaveData>(json);
        if (saveData == null)
        {
            saveData = new AchievementSaveData();
        }

        // Dictionary는 JsonUtility가 기본 지원을 하지 않아 수동으로 초기화
        if (saveData.statistics == null)
            saveData.statistics = new Dictionary<string, long>();
        if (saveData.claimedAchievementIds == null)
            saveData.claimedAchievementIds = new List<string>();
        if (saveData.completedAchievementIds == null)
            saveData.completedAchievementIds = new List<string>();

        Debug.Log("업적 데이터 로드 완료.");
    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(saveKey, json);
        Debug.Log("업적 데이터 저장 완료.");
    }

    // ------------ 여기서 부터는 외부 호출 함수 ------------

    /// <summary>
    /// 지정된 통계(Stat)의 값을 증가시킴
    /// </summary>
    /// <param name="statId">"totalKills", "gachaSpent" 등 AchievementData에 정의된 ID</param>
    /// <param name="amount">증가량</param>
    public void IncrementStat(string statId, long amount = 1)
    {
        if (string.IsNullOrEmpty(statId)) return;

        // 1. 통계 값 증가
        long currentValue = GetStatValue(statId);
        currentValue += amount;
        saveData.statistics[statId] = currentValue;

        Debug.Log($"통계 갱신: {statId} = {currentValue}");

        // 2. 통계를 추적하는 업적이 있는지 확인
        CheckForCompletion(statId, currentValue);

        // 3. 즉시 저장
        SaveData();
    }

    // 완료된 업적의 보상 수령 (UI 버튼에서 호출)
    public void ClaimReward(string achievementId)
    {
        AchievementData data = GetAchievementByID(achievementId);
        if (data == null)
        {
            Debug.LogError($"ID가 {achievementId}인 업적을 찾을 수 없습니다.");
            return;
        }

        // 이미 수령했거나, 완료되지 않았으면 거부
        if (saveData.claimedAchievementIds.Contains(achievementId)) return;
        if (!saveData.completedAchievementIds.Contains(achievementId)) return;

        // 1. 보상 지급
        GiveReward(data);

        // 2. '수령 완료' 목록에 추가
        saveData.claimedAchievementIds.Add(achievementId);

        // 3. 완료 목록에서는 제거
        saveData.completedAchievementIds.Remove(achievementId);

        Debug.Log($"보상 수령: {data.title}");

        // 4. UI 갱신을 위해 이벤트 발송
        OnAchievementClaimed?.Invoke();

        SaveData();
    }

    // ------------ 여기서 부터는 내부 함수 ------------

    void CheckForCompletion(string statId, long currentValue)
    {
        // 해당 통계를 추적하는 모든 업적 찾기
        foreach (var ach in allAchievements.Where(a => a.statToTrack == statId))
        {
            // 이미 완료했거나 수령했다면 건너뜀
            if (saveData.completedAchievementIds.Contains(ach.id) ||
                saveData.claimedAchievementIds.Contains(ach.id))
            {
                continue;
            }

            // 목표치 달성
            if (currentValue >= ach.targetValue)
            {
                saveData.completedAchievementIds.Add(ach.id);
                OnAchievementCompleted?.Invoke(ach);    // 업적이 달성 됬음을 알림
                Debug.LogWarning($"업적 달성: {ach.title}");
            }
        }
    }

    void GiveReward(AchievementData data)
    {
        switch (data.rewardType)
        {
            case RewardType.PremiumCurrency:
                // TODO: 재화 관리자에게 재화를 추가하라고 요청
                // 예: CurrencyManager.instance.AddPremiumCurrency(data.rewardAmount);
                Debug.Log($"보상: 잼 획득 {data.rewardAmount}개");
                break;
            case RewardType.CharacterUnlock:
                // TODO: 캐릭터 해금 로직 (기존 AchiveManager의 PlayerPrefs 방식 개선)
                // 예: CharacterUnlockManager.instance.UnlockCharacter(data.characterUnlockId);
                Debug.Log($"보상: 캐릭터 {data.characterUnlockId} 해금");
                break;
        }
    }

    // ------------ 여기서 부터는 UI에서 사용할 데이터 조회 함수 ------------

    public AchievementData GetAchievementByID(string id)
    {
        return allAchievements.FirstOrDefault(a => a.id == id);
    }

    public long GetStatValue(string statId)
    {
        saveData.statistics.TryGetValue(statId, out long value);
        return value;
    }

    // 업적의 현재 상태 (UI 표시에 사용)
    public enum AchievementStatus { InProgress, Completed, Claimed }

    public AchievementStatus GetAchievementStatus(AchievementData data)
    {
        if (saveData.claimedAchievementIds.Contains(data.id))
            return AchievementStatus.Claimed;

        if (saveData.completedAchievementIds.Contains(data.id))
            return AchievementStatus.Completed;

        return AchievementStatus.InProgress;
    }

    private void OicationQuit()
    {
        SaveData();    
    }
}