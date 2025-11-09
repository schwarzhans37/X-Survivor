using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementUI : MonoBehaviour
{
    public Transform contentRoot;   // 업적 슬롯이 생성될 부모
    public GameObject achievementSlotPrefab;

    private List<AchievementSlotUI> spawnedSlots = new List<AchievementSlotUI>();

    void OnEnable()
    {
        // 탭을 열 때마다 항상 최신 정보로 갱신
        // 기본적으로 '전체' 탭 보여주기
        PopulateList(AchievementCategory.Overall);

        //보상 수령 이벤트가 발생하면 리스트 갱신
        AchievementManager.OnAchievementClaimed += HandleClaimEvent;
    }

    void OnDisable()
    {
        AchievementManager.OnAchievementClaimed -= HandleClaimEvent;
    }

    // 보상 수령 시, 개별 슬롯을 찾아서 갱신
    void HandleClaimEvent()
    {
        foreach (var slot in spawnedSlots)
        {
            slot.Refresh();
        }
    }

    // 탭 버튼에서 이 함수들을 호출
    public void ShowOverallTab()
    {
        PopulateList(AchievementCategory.Overall);
    }

    public void ShowCombatTab()
    {
        PopulateList(AchievementCategory.Combat);
    }

    public void ShowSpendingTab()
    {
        PopulateList(AchievementCategory.Spending);
    }

    // 리스트 채우기 로직
    void PopulateList(AchievementCategory category)
    {
        // 1. 기존 슬롯 모두 숨기기 혹은 파괴
        foreach (var slot in spawnedSlots)
        {
            slot.gameObject.SetActive(false);
        }

        // 2. 매니저로부터 해당 카테고리의 모든 업적 데이터 가져오기
        List<AchievementData> achievementsToShow = new List<AchievementData>();
        foreach (var achData in AchievementManager.instance.allAchievements)
        {
            if (achData.category == category)
            {
                achievementsToShow.Add(achData);
            }
        }

        // 정렬 (미완료 -> 완료 순으로 정렬)
        achievementsToShow.Sort((a, b) =>
            AchievementManager.instance.GetAchievementStatus(a)
            .CompareTo(AchievementManager.instance.GetAchievementStatus(b))
        );

        // 3. 리스트에 슬롯 생성 및 설정
        for (int i = 0; i <achievementsToShow.Count; i++)
        {
            AchievementSlotUI slot;
            if (i < spawnedSlots.Count)
            {
                // 기존 슬롯 재활용
                slot = spawnedSlots[i];
                slot.gameObject.SetActive(true);
            }
            else
            {
                // 새 슬롯 생성
                GameObject newSlotObj = Instantiate(achievementSlotPrefab, contentRoot);
                slot = newSlotObj.GetComponent<AchievementSlotUI>();
                spawnedSlots.Add(slot);
            }

            slot.Setup(achievementsToShow[i]);
        }
    }
}
