using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetSelectionUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Transform gridContainer;     // 펫 슬롯들이 생성될 부모(Grid 오브젝트)
    public GameObject petSlotPrefab;    // 사용할 펫 슬롯 프리팹
    public Button startButton;          // 시작(START)버튼

    private PetData selectedPet;        // 현재 선택된 펫의 데이터

    // 이 UI 패널이 활성화될 때마다 호출
    void OnEnable()
    {
        RefreshUI();
    }

    // UI를 최신 데이터로 새로고침 하는 함수
    public void RefreshUI()
    {
        // 1. 기존에 생성된 슬롯들을 모두 삭제
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // 데이터가 아직 로드되지 않았을 경우를 대비해 null 체크
        if (PlayerDataManager.instance == null || PlayerDataManager.instance.playerData == null) return;

        // 2. 플레이어가 보유한 펫 목록을 불러옴
        var ownedPetIDs = PlayerDataManager.instance.playerData.ownedPetIDs;
        var petDB = PlayerDataManager.instance.petDatabase;     // 데이터베이스 참조 가져오기

        // 3. 보유한 펫만큼 슬롯을 동적으로 생성
        foreach (int petId in ownedPetIDs)
        {
            PetData data = petDB.GetPetByID(petId);     // 데이터베이스에서 검색
            if (data != null)
            {
                GameObject slotGO = Instantiate(petSlotPrefab, gridContainer);
                slotGO.GetComponent<PetSlot>().Setup(data, this);
            }
        }

        // 4. 초기 상태 설정
        selectedPet = null;
        startButton.interactable = false;   // 처음에는 START 버튼 비활성화
    }

    // PetSlot이 클릭되었을 때 호출되는 함수
    public void OnPetSelected(PetData petData)
    {
        selectedPet = petData;
        Debug.Log($"[{selectedPet.grade}] {selectedPet.petName} 선택됨");

        // START 버튼 활성화
        startButton.interactable = true;
    }

    // START 버튼을 클릭했을 때 호출되는 함수
    public void ConfirmSelection()
    {
        if (selectedPet == null)
        {
            Debug.LogError("선택된 펫이 없습니다.");
            return;
        }

        // 선택된 펫 ID를 PlayerDataManager에 저장
        PlayerDataManager.instance.playerData.selectedPetId = selectedPet.id;

        // 게임 플레이 씬 로드
        MenuManager.instance.StartGame();
    }
}
