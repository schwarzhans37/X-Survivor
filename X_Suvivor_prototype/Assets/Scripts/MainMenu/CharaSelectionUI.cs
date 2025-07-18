using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharaSelectionUI : MonoBehaviour
{
    [Header("UI 구성요소")]
    public CharaSlotUI[] charaSlots;       // 모든 캐릭터 슬롯을 담는 ㅐㅂ열
    public Button nextButton;

    private CharaSlotUI currentlySelectedSlot;  // 현재 선택된 슬롯

    // 이 패널이 활성화될 때마다 UI를 새로고침
    void OnEnable()
    {
        RefreshUI();
    }

    // UI를 최신 플레이어 데이터 기준으로 업데이트
    public void RefreshUI()
    {
        // 1. 플레이어가 해금한 캐릭터 ID 목록을 가져옴
        List<int> unlockedIDs = PlayerDataManager.instance.playerData.unlockedCharacterIDs;

        // 2. 모든 캐릭터 슬롯을 순회하며 상태 업데이트
        foreach (CharaSlotUI slot in charaSlots)
        {
            slot.Initialize(this);  // 각 슬롯 초기화
            bool isUnlocked = unlockedIDs.Contains(slot.charaId);
            slot.SetLockState(isUnlocked);
        }

        // 3. 초기 선택 상태 초기화
        currentlySelectedSlot = null;
        nextButton.interactable = false;    // 처음에는 NEXT 버튼 비활성화
    }

    // 슬롯이 클릭되었을 때 CharaSlotUI에 의해 호출됨
    public void OnCharaSelected(CharaSlotUI selectedSlot)
    {
        // 이전에 선택된 슬롯이 있었다면 선택 해제
        if (currentlySelectedSlot != null)
        {
            currentlySelectedSlot.Deselect();
        }

        // 새로 선택된 슬롯을 기록하고 선택 효과 표시
        currentlySelectedSlot = selectedSlot;
        currentlySelectedSlot.Select();

        Debug.Log($"캐릭터 ID: {selectedSlot.charaId} 선택됨");

        // NEXT 버튼 활성화
        nextButton.interactable = true;
    }

    // NEXT 버튼이 눌렸을 때 MenuManager가 호출할 함수
    public void ConfirmSelection()
    {
        if (currentlySelectedSlot == null) return;

        // 최종 선택된 캐릭터 ID를 데이터 관리자에 저장
        PlayerDataManager.instance.playerData.selectedCharacterId = currentlySelectedSlot.charaId;

        // MenuManager에게 펫 선택 페이지로 넘겨달라고 요청
        MenuManager.instance.ShowPetSelectPage();
    }
}
