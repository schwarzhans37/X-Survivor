using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using System.Linq;

public class LevelUp : MonoBehaviour
{
    public List<WeaponData> weaponDatas; // 인스펙터에서 모든 무기 데이터 연결
    public List<GearData> gearDatas;   // 인스펙터에서 모든 장비 데이터 연결
    public List<PetUpgradeData> petUpgradeDatas;    // 인스펙터에서 모든 펫 강화 데이터 연결
    RectTransform rect;
    private Item[] items; // 레벨업 슬롯 UI들

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx("LevelUp");
        AudioManager.instance.EffectBgm(true);
    }

    public void hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx("Select");
        AudioManager.instance.EffectBgm(false);
    }

    public void Select(int index)
    {
        items[index].OnClick();
    }

   void Next()
    {
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        Player player = GameManager.instance.player;

        // 현재 씬에 있는 펫 컨트롤러를 찾음. 없으면 null
        PetController pet = FindObjectOfType<PetController>();

        // ===== 1. 레벨업 가능한 후보 아이템 목록 생성 =====
        List<object> candidateItems = new List<object>();

        // ----- 후보 1: 아직 획득하지 않은 무기들 -----
        List<WeaponData> availableWeapons = new List<WeaponData>();
        foreach (WeaponData data in weaponDatas)
        {
            // 플레이어가 이 무기를 가지고 있지 않다면 후보에 추가
            if (player.FindEquippedWeapon(data) == null)
            {
                availableWeapons.Add(data);
            }
        }
        candidateItems.AddRange(availableWeapons);

        // ----- 후보 2: 이미 획득했지만, 최대 레벨이 아닌 무기들 -----
        foreach (WeaponBase equippedWeapon in player.equippedWeapons)
        {
            // 최대 레벨이 아니라면 후보에 추가
            if (equippedWeapon.currentLevel < equippedWeapon.weaponData.damages.Length)
            {
                candidateItems.Add(equippedWeapon.weaponData);
            }
        }

        // ----- 후보 3 & 4: 장비 (위와 동일한 로직 적용) -----
        // (Player.cs에 FindEquippedGear 함수가 완성되었다고 가정)
        List<GearData> availableGears = new List<GearData>();
        foreach(GearData data in gearDatas)
        {
            if (player.FindEquippedGear(data) == null)
            {
                availableGears.Add(data);
            }
        }
        candidateItems.AddRange(availableGears);

        foreach (Gear equippedGear in player.equippedGears)
        {
            if (equippedGear.currentLevel < equippedGear.gearData.Values.Length)
            {
                candidateItems.Add(equippedGear.gearData);
            }
        }
        
        // ----- 후보 5: 펫 강화 -----
        // 펫이 존재하고, 살아있을 때만 펫 강화 항목을 후보에 추가
        if (pet != null && pet.IsAlive)
        {
            foreach (PetUpgradeData data in petUpgradeDatas)
            {
                // PetController에게 이 강화의 현재 레벨을 물어봄
                int currentLevel = pet.GetUpgradeLevel(data.upgradeType);

                // 현재 레벨이 최대 레벨보다 낮으면 후보에 추가
                if (currentLevel <data.upgradeValues.Length)
                {
                    candidateItems.Add(data);
                }
            }
        } 
        
        // ===============================================

        // 2. 이 중에서 랜덤하게 3개를 중복 없이 뽑습니다.
        // 후보가 3개 미만일 수도 있으므로, Math.Min을 사용해 안전하게 처리
        int numToSelect = Mathf.Min(3, candidateItems.Count);
        List<object> selectedItems = candidateItems.OrderBy(x => Random.value).Take(numToSelect).ToList();

        // 3. 뽑힌 아이템들을 UI 슬롯에 할당합니다.
        for (int i = 0; i < selectedItems.Count; i++)
        {
            object selectedData = selectedItems[i];
            Item uiSlot = items[i];

            if (selectedData is WeaponData)
            {
                uiSlot.Init((WeaponData)selectedData);
            }
            else if (selectedData is GearData)
            {
                uiSlot.Init((GearData)selectedData);
            }
            else if (selectedData is PetUpgradeData)
            {
                uiSlot.Init((PetUpgradeData)selectedData);
            }
            
            uiSlot.gameObject.SetActive(true);
        }
    }
}
