using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 하나의 인벤토리 슬롯 정보를 담는 클래스
public class InventorySlot
{
    public ItemSkillData data;
    public int count;
    public MonoBehaviour skillInstance;

    public bool IsOnCooldown()
    {
        return GetCooldownTimer() > 0f;
    }

    public float GetCooldownRatio()
    {
        if (skillInstance == null) return 0f;
        switch (data.skillType)
        {
            case ItemSkillType.Lightning: return ((LightningSkill)skillInstance).GetCooldownRatio();
            case ItemSkillType.Bomb: return ((BombSkill)skillInstance).GetCooldownRatio();
            case ItemSkillType.MagneticField: return ((MagneticFieldSkill)skillInstance).GetCooldownRatio();
            case ItemSkillType.SplitArrows: return ((SplitArrowsSkill)skillInstance).GetCooldownRatio();
            default: return 0f;
        }
    }

    public float GetCooldownTimer()
    {
        if (skillInstance == null) return 0f;
        switch (data.skillType)
        {
            case ItemSkillType.Lightning: return ((LightningSkill)skillInstance).cooldownTimer;
            case ItemSkillType.Bomb: return ((BombSkill)skillInstance).cooldownTimer;
            case ItemSkillType.MagneticField: return ((MagneticFieldSkill)skillInstance).cooldownTimer;
            case ItemSkillType.SplitArrows: return ((SplitArrowsSkill)skillInstance).cooldownTimer;
            default: return 0f;
        }
    }
}

public class SkillInventory : MonoBehaviour
{
    [Header("슬롯 설정")]
    public int maxSlots = 4;
    public List<InventorySlot> slots = new List<InventorySlot>();

    [Header("UI 연결")]
    public SkillSlotUI[] uiSlots;

    void Start()
    {
        foreach (var uiSlot in uiSlots)
        {
            if (uiSlot != null) uiSlot.ClearSlot();
        }
    }

    public void AddItem(ItemSkillData skillData)
    {
        // 1. 이미 같은 스킬을 가지고 있는지 확인
        foreach (var slot in slots)
        {
            if (slot.data == skillData)
            {
                slot.count++; // 이미 있다면 갯수만 증가
                UpdateAllUISlots();
                return; // 함수 종료
            }
        }

        // 2. 새로 획득한 스킬이고, 빈 슬롯이 있을 경우
        if (slots.Count < maxSlots)
        {
            GameObject skillObj = Instantiate(skillData.skillPrefab, transform);
            MonoBehaviour instance = skillObj.GetComponent<MonoBehaviour>();

            InventorySlot newSlot = new InventorySlot
            {
                data = skillData,
                count = 1,
                skillInstance = instance
            };
            slots.Add(newSlot);
            UpdateAllUISlots();
        }
    }

    void OnUseInventory1(InputValue value)
    {
        if (!value.isPressed) return;
        UseSkill(0);
    }

    void OnUseInventory2(InputValue value)
    {
        if (!value.isPressed) return;
        UseSkill(1);
    }

    void OnUseInventory3(InputValue value)
    {
        if (!value.isPressed) return;
        UseSkill(2);
    }

    void OnUseInventory4(InputValue value)
    {
        if (!value.isPressed) return;
        UseSkill(3);
    }

    private void UseSkill(int slotIndex)
    {
        if (slotIndex < slots.Count && slots[slotIndex].count > 0)
        {
            var slot = slots[slotIndex];
            bool success = false;

            switch (slot.data.skillType)
            {
                case ItemSkillType.Lightning:     success = ((LightningSkill)slot.skillInstance).TryUse(); break;
                case ItemSkillType.Bomb:          success = ((BombSkill)slot.skillInstance).TryUse(); break;
                case ItemSkillType.MagneticField: success = ((MagneticFieldSkill)slot.skillInstance).TryUse(); break;
                case ItemSkillType.SplitArrows:   success = ((SplitArrowsSkill)slot.skillInstance).TryUse(); break;
            }

            if (success)
            {
                slot.count--;
                UpdateAllUISlots(); 
            }
        }
    }

    void UpdateAllUISlots()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < slots.Count)
            {
                // 갯수가 0이 되면 슬롯 자체를 비우도록 로직 변경
                if (slots[i].count > 0)
                {
                    uiSlots[i].UpdateSlot(slots[i]);
                }
                else
                {
                    // 갯수는 0이지만, 슬롯 정보는 남아있는 상태. UI만 비워줌
                    uiSlots[i].ClearSlot();
                }
            }
            else
            {
                uiSlots[i].ClearSlot();
            }
        }
    }
}