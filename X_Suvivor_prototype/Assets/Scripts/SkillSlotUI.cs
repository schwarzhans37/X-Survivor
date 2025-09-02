using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Image icon;
    public Image cooldownImage;
    public Text countText;
    public GameObject keyHint; // '1', '2' 등이 쓰인 키보드 이미지

    private InventorySlot currentSlot;

    void Update()
    {
        // 슬롯에 스킬이 할당되어 있고, 쿨타임 중일 때만 UI 업데이트
        if (currentSlot != null && currentSlot.count > 0)
        {
            // TODO: 각 스킬 스크립트에서 쿨타임 비율을 가져오는 통일된 방법 필요
            // float ratio = currentSlot.GetCooldownRatio();
            // cooldownImage.fillAmount = ratio;
        }
    }

    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;
        gameObject.SetActive(true);

        icon.sprite = slot.data.skillIcon;
        icon.color = new Color(1, 1, 1, 1); // 아이콘 활성화
        countText.text = slot.count.ToString();
        countText.gameObject.SetActive(true);
    }

    public void ClearSlot()
    {
        currentSlot = null;
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0.5f); // 아이콘 비활성화
        countText.gameObject.SetActive(false);
        cooldownImage.fillAmount = 0;
    }
}
