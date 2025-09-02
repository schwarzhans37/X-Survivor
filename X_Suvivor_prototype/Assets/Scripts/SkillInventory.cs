using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 하나의 인벤토리 슬롯 정보를 담는 클래스
public class InventorySlot
{
    public ItemSkillData data;
    public int count;
    public MonoBehaviour skillInstance; // 실제 스킬 컴포넌트 참조

    public bool IsOnCooldown()
    {
        if (skillInstance == null) return true;
        // 각 스킬 스크립트의 public 함수를 호출하여 쿨타임 상태를 가져옴 (추후 수정 필요)
        if (data.skillType == ItemSkillType.Lightning) return ((LightningSkill)skillInstance).cooldownTimer > 0f;
        if (data.skillType == ItemSkillType.Bomb) return ((BombSkill)skillInstance).cooldownTimer > 0f;
        if (data.skillType == ItemSkillType.MagneticField) return ((MagneticFieldSkill)skillInstance).cooldownTimer > 0f;
        if (data.skillType == ItemSkillType.SplitArrows) return ((SplitArrowsSkill)skillInstance).cooldownTimer > 0f;
        // ... 다른 스킬들도 추가 ...
        return true;
    }
}

public class SkillInventory : MonoBehaviour
{
    [Header("슬롯 설정")]
    public int maxSlots = 4;
    public List<InventorySlot> slots = new List<InventorySlot>();

    [Header("UI 연결")]
    public SkillSlotUI[] uiSlots; // 인스펙터에서 4개의 UI 슬롯 연결

    void Start()
    {
        // 초기 슬롯 상태 UI에 반영
        UpdateAllUISlots();
    }

    // 아이템 획득 함수 (TreasureChest 등에서 호출)
    public void AddItem(ItemSkillData skillData)
    {
        // 1. 이미 같은 스킬을 가지고 있는지 확인
        foreach (var slot in slots)
        {
            if (slot.data == skillData)
            {
                slot.count++;
                UpdateAllUISlots();
                return;
            }
        }

        // 2. 빈 슬롯이 있는지 확인
        if (slots.Count < maxSlots)
        {
            // 새 스킬 프리팹을 플레이어의 자식으로 생성
            GameObject skillObj = Instantiate(skillData.skillPrefab, transform);
            MonoBehaviour instance = skillObj.GetComponent<MonoBehaviour>(); // LightningSkill 등 부모 클래스로 받기

            // 새 슬롯 추가
            InventorySlot newSlot = new InventorySlot
            {
                data = skillData,
                count = 1,
                skillInstance = instance
            };
            slots.Add(newSlot);
            UpdateAllUISlots();
        }
        // 인벤토리가 꽉 찼으면 아무것도 안 함
    }

    // Player Input 컴포넌트에서 호출될 함수들
    public void OnUseSkill1(InputValue value) { UseSkill(0); }
    public void OnUseSkill2(InputValue value) { UseSkill(1); }
    public void OnUseSkill3(InputValue value) { UseSkill(2); }
    public void OnUseSkill4(InputValue value) { UseSkill(3); }

    private void UseSkill(int slotIndex)
    {
        if (slotIndex < slots.Count && slots[slotIndex].count > 0)
        {
            var slot = slots[slotIndex];
            bool success = false;

            // 각 스킬 스크립트의 public 사용 함수 호출
            switch (slot.data.skillType)
            {
                case ItemSkillType.Lightning:     success = ((LightningSkill)slot.skillInstance).TryUse(); break;
                // BombSkill 등은 입력 함수가 아닌 public 사용 함수로 변경 필요
                // case SkillType.Bomb:       success = ((BombSkill)slot.skillInstance).TryUse(); break;
                // ...
            }

            if (success)
            {
                slot.count--;
                if (slot.count <= 0)
                {
                    // 갯수가 0이 되면 슬롯에서 제거
                    Destroy(((MonoBehaviour)slot.skillInstance).gameObject);
                    slots.RemoveAt(slotIndex);
                }
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
                // 채워진 슬롯 정보로 UI 업데이트
                uiSlots[i].UpdateSlot(slots[i]);
            }
            else
            {
                // 빈 슬롯 UI 처리
                uiSlots[i].ClearSlot();
            }
        }
    }
}
