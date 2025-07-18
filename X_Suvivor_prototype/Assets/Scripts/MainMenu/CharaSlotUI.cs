using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;

public class CharaSlotUI : MonoBehaviour
{
    [Header("슬롯 정보")]
    public int charaId;      // 인스펙터에서 각 슬롯에 고유 ID 할당

    [Header("UI 요소")]
    public Image charaImage;        // 캐릭터 초상화 이미지
    public Button slotButton;       // 슬롯 자체 버튼
    public GameObject selectionOutline;     // 선택되었을 때 표시될 외곽선 (아직 에셋 없음)

    [Header("잠금 상태")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);   // 잠겼을 때 색상
    public Sprite lockedSprite; // 잠겼을 때 실루엣 이미지 (아직 에셋 없음)

    private CharaSelectionUI uiManager;     // 이 슬롯을 관리하는 상위 UI

    // 상위 UI 관리자가 이 슬롯을 초기화할 때 호출
    public void Initialize(CharaSelectionUI manager)
    {
        this.uiManager = manager;
        slotButton.onClick.AddListener(OnSlotClicked);
        Deselect(); // 처음에는 비선택 상태
    }

    // 잠금/해제 상태에 따라 UI 업데이트
    public void SetLockState(bool isUnlocked)
    {
        slotButton.interactable = isUnlocked;

        if (isUnlocked)
        {
            charaImage.color = Color.white; // 원래 색상으로
        }
        else
        {
            charaImage.color = lockedColor;
            if (lockedSprite != null)
            {
                charaImage.sprite = lockedSprite;
            }
        }
    }

    // 슬롯이 클릭되었을 때 호출
    private void OnSlotClicked()
    {
        uiManager.OnCharaSelected(this);
    }

    // 선택 효과 표시
    public void Select()
    {
        selectionOutline.SetActive(true);
    }

    // 선택 효과 숨김
    public void Deselect()
    {
        selectionOutline.SetActive(false);
    }
}
