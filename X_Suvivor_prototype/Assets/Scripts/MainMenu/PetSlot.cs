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

        if (data.petIcon != null)
        {
            // Image 컴포넌트의 sprite 속성에 PetData의 아이콘을 할당
            petIcon.sprite = data.petIcon;
            petIcon.color = Color.white;
        }
        else
        {
            // 아이콘이 없다면 비활성화하거나 기본 이미지 표시
            petIcon.color = new Color(1, 1, 1, 0);  // 투명하게 만듦
        }
    }

    // 이 슬롯이 클릭이 되었을 경우
    private void OnSelectThisPet()
    {
        // UI 관리자에게 "내가 선택되었다." 라고 알림
        uiManager.OnPetSelected(this.petData);
    }
}
