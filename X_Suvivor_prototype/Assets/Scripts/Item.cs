using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    // 아이템의 종류를 식별하기 위한 enum
    public enum ItemCategory { Weapon, Gear };
    public ItemCategory itemCategory;

    // 데이터를 담을 변수, 인스펙터에서는 하나만 사용됨
    [Header("Data")]
    public WeaponData weaponData;
    public GearData gearData;
    public int currentDisplayLevel;   // 이 아이템 UI가 현재 표시하는 레벨


    [Header("UI 연결")]
    [Tooltip("레벨업 메뉴에 등장할 성장 요소들의 내용물을 설정합니다.")]
    public Image icon;
    public Text textLevel;
    public Text textName;
    public Text textDesc;

    void Awake()
    {
        // UI 컴포넌트들을 자식에서 찾아 할당
        icon = transform.Find("Icon").GetComponent<Image>();
        textLevel = transform.Find("LevelText").GetComponent<Text>();
        textName = transform.Find("NameText").GetComponent<Text>();
        textDesc = transform.Find("DescText").GetComponent<Text>();
    }

    // LevelUp 스크립트가 이 함수를 호출하여 아이템 정보를 설정
    public void Init(WeaponData data)
    {
        itemCategory = ItemCategory.Weapon;
        this.weaponData = data;
        UpdateUI();
    }

    public void Init(GearData data)
    {
        itemCategory = ItemCategory.Gear;
        this.gearData = data;
        UpdateUI();
    }

    // 현재 정보에 맞게 UI 텍스트와 아이콘을 업데이트
    public void UpdateUI()
    {
        // Player를 찾아 현재 장착된 아이템의 레벨을 가져옴
        Player player = GameManager.instance.player;

        switch (itemCategory)
        {
            case ItemCategory.Weapon:
                // 플레이어가 가진 무기의 실제 레벨을 가져와야 함
                WeaponBase equippedWeapon = player.FindEquippedWeapon(weaponData);
                currentDisplayLevel = equippedWeapon != null ? equippedWeapon.currentLevel : -1;

                icon.sprite = weaponData.weaponIcon;
                textName.text = weaponData.weaponName;
                textLevel.text = "Lv." + (currentDisplayLevel + 2);
                // TODO: 레벨에 맞는 설명 업데이트
                // textDesc.text = ...
                break;
            case ItemCategory.Gear:
                // 플레이어가 가진 장비의 실제 레벨을 가져와야 함
                Gear equippedGear = player.FindEquippedGear(gearData);
                currentDisplayLevel = equippedGear != null ? equippedGear.currentLevel : -1;

                icon.sprite = gearData.gearIcon;
                textName.text = gearData.gearName;
                textLevel.text = "Lv." + (currentDisplayLevel + 2);
                // textDesc.text = string.Format(gearData.gearDesc, ...);
                break;
        }
    }

    // 레벨업 버튼 클릭 시
    public void OnClick()
    {
        Player player = GameManager.instance.player;

        switch (itemCategory)
        {
            case ItemCategory.Weapon:
                WeaponBase existingWeapon = player.FindEquippedWeapon(weaponData);
                if (existingWeapon == null)
                {
                    player.EquipWeapon(weaponData);
                }
                else
                {
                    existingWeapon.LevelUp();
                }
                break;
            case ItemCategory.Gear:
                if (gearData.gearType == GearData.GearType.Heal)
                {
                    player.Heal();
                    Debug.Log("체력을 모두 회복합니다.");
                }
                else
                {
                    Gear existingGear = player.FindEquippedGear(gearData);
                    if (existingGear == null)
                    {
                        player.EquipGear(gearData);
                    }
                    else
                    {
                        existingGear.LevelUp();
                    }
                }
                break;
        }
        // 레벨업 UI를 닫는 로직
        GameManager.instance.uiLevelUo.hide();
    }
}
