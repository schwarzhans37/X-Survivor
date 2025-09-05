using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public Button skillButton;
    public Text countText;
    public Text cooldownText;

    private InventorySlot currentSlot;

    void Update()
    {
        // 슬롯에 스킬이 할당되어 있을 때만 업데이트
        if (currentSlot != null && currentSlot.skillInstance != null)
        {
            // 쿨타임이 돌고 있는지 여부 확인
            bool onCooldown = currentSlot.IsOnCooldown();
            
            // 버튼의 상호작용 가능 상태를 쿨타임이 아닌 상태와 일치시킴
            // (쿨타임 중이면 false, 아니면 true)
            skillButton.interactable = !onCooldown;
            
            // 쿨타임 텍스트의 활성화 상태를 쿨타임 중인 상태와 일치시킴
            cooldownText.gameObject.SetActive(onCooldown);

            if (onCooldown)
            {
                // 쿨타임 중일 때만 텍스트 업데이트
                cooldownText.text = currentSlot.GetCooldownTimer().ToString("F1");
            }
        }
    }

    // SkillInventory가 이 함수를 호출하여 슬롯 정보를 갱신
    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;
        gameObject.SetActive(true);

        // 버튼의 Image 컴포넌트에 아이콘을 설정
        skillButton.image.sprite = slot.data.skillIcon;
        skillButton.image.color = new Color(1, 1, 1, 1); // 아이콘 활성화

        if (countText != null)
        {
            countText.text = slot.count.ToString();
            countText.gameObject.SetActive(true);
        }
        
        // 처음 업데이트 시에는 쿨타임이 아니므로 버튼 활성화 및 텍스트 숨김
        skillButton.interactable = true;
        cooldownText.gameObject.SetActive(false);
    }

    // 슬롯이 비워질 때 호출
    public void ClearSlot()
    {
        currentSlot = null;
        
        skillButton.image.sprite = null;
        skillButton.image.color = new Color(1, 1, 1, 0.0f); // 아이콘 비활성화 (반투명)
        skillButton.interactable = false; // 빈 슬롯은 클릭 불가

        if (countText != null)
        {
            countText.gameObject.SetActive(false);
        }
        
        cooldownText.gameObject.SetActive(false);
    }
}