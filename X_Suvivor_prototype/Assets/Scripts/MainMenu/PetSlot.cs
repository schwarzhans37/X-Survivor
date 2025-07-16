using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSlot : MonoBehaviour
{
    public Image petIcon;
    public Button selectButton;
    private PetData petData;
    private PetSelectionUI uiManager;   //이 슬롯을 관리하는 메인 UI 관리자

    void Awake()
    {
        // 버튼에 클릭 이벤트 리스너를 코드로 추가
        selectButton.onClick.AddListener(OnSelectThisPet);
    }

    // PetSelectionUI 관리자가 이 슬롯을 초기화할 때 호출하는 함수
    public void Setup(PetData data, PetSelectionUI manager)
    {
        this.petData = data;
        this.uiManager = manager;

        // 아이콘 설정 (PetDatabase에서 아이콘 Sprite를 가져와야 함)
        //petIcon.sprite = PetDatabase.instance.GetSpriteForPet(data.Id);
        petIcon.color = GetColorByGrade(data.Grade); // 임시로 등급별 색상 표시
    }

    // 이 슬롯이 클릭이 되었을 경우
    private void OnSelectThisPet()
    {
        // UI 관리자에게 "내가 선택되었다." 라고 알림
        uiManager.OnPetSelected(this.petData);
    }

    // 등급에 따라 색상을 반환하는 임시 함수
    private Color GetColorByGrade(string grade)
    {
        switch (grade)
        {
            case "레전드": return Color.yellow;
            case "유니크": return new Color(1f, 0.5f, 0f);
            case "에픽": return Color.magenta;
            case "레어": return Color.blue;
            case "노멀": default: return Color.white;
        }
    }
}
