using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public enum ItemCategory { Weapon, Gear };
    public ItemCategory itemCategory;

    public WeaponData weaponData;
    public GearData gearData;

    // 이 슬롯이 “다음에 오를 레벨”을 표시하기 위해 내부적으로 가진 현재 레벨
    public int currentDisplayLevel;

    [Header("UI 연결")]
    public Image icon;
    public Text textLevel;
    public Text textName;
    public Text textDesc;

    void Awake()
    {
        // 인스펙터에 미리 연결돼 있으면 건들지 않음. 비어있으면 이름으로 탐색 (씬마다 이름이 다른 경우 대비)
        if (!icon)
            icon = transform.Find("Icon")?.GetComponent<Image>();

        if (!textLevel)
            textLevel = transform.Find("LevelText")?.GetComponent<Text>()
                     ?? transform.Find("Text Level")?.GetComponent<Text>();

        if (!textName)
            textName = transform.Find("NameText")?.GetComponent<Text>()
                     ?? transform.Find("Text Name")?.GetComponent<Text>();

        if (!textDesc)
            textDesc = transform.Find("DescText")?.GetComponent<Text>()
                     ?? transform.Find("Text Desc")?.GetComponent<Text>();
    }

    // LevelUp이 호출
    public void Init(WeaponData data)
    {
        itemCategory = ItemCategory.Weapon;
        weaponData = data;
        gearData = null;
        UpdateUI();
    }

    public void Init(GearData data)
    {
        itemCategory = ItemCategory.Gear;
        gearData = data;
        weaponData = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        var player = GameManager.instance.player;

        switch (itemCategory)
        {
            case ItemCategory.Weapon:
                {
                    // 1) 기존 방식 우선
                    WeaponBase equipped = player.FindEquippedWeapon(weaponData);

                    // 2) 못 찾으면 이름으로 직접 스캔 (Player.cs 수정 없이 보완)
                    if (equipped == null && player.equippedWeapons != null)
                    {
                        foreach (var w in player.equippedWeapons)
                        {
                            if (w && w.weaponData &&
                                w.weaponData.weaponName == weaponData.weaponName)
                            {
                                equipped = w; break;
                            }
                        }
                    }

                    int curLv = equipped ? equipped.currentLevel : -1;   // 미보유: -1
                    int nextLv = curLv + 2;                               // 표시용(1-base) + 다음레벨

                    if (icon) icon.sprite = weaponData.weaponIcon;
                    if (textName) textName.text = weaponData.weaponName;
                    if (textLevel) textLevel.text = $"Lv.{nextLv}";
                    if (textDesc) textDesc.text = BuildWeaponDesc(weaponData, curLv + 1); // 다음레벨(1-base)

                    break;
                }

            case ItemCategory.Gear:
                {
                    Gear equipped = player.FindEquippedGear(gearData);

                    if (equipped == null && player.equippedGears != null)
                    {
                        foreach (var g in player.equippedGears)
                        {
                            if (g && g.gearData &&
                                (g.gearData.gearName == gearData.gearName ||
                                 g.type == gearData.gearType))              // 타입으로도 보조 매칭
                            {
                                equipped = g; break;
                            }
                        }
                    }

                    int curLv = equipped ? equipped.currentLevel : -1;
                    int nextLv = curLv + 2;

                    if (icon) icon.sprite = gearData.gearIcon;
                    if (textName) textName.text = gearData.gearName;
                    if (textLevel) textLevel.text = $"Lv.{nextLv}";
                    if (textDesc) textDesc.text = BuildGearDesc(gearData, curLv + 1);

                    break;
                }
        }
    }

    // ===== 설명 생성기 (예: 다음 레벨 수치 보여주기) =====
    string BuildWeaponDesc(WeaponData data, int nextLv1Based)
    {
        string line = !string.IsNullOrEmpty(data.weaponDesc) ? data.weaponDesc : "무기 강화";

        if (data.damages != null && data.damages.Length > 0)
        {
            int idx = Mathf.Clamp(nextLv1Based - 1, 0, data.damages.Length - 1);
            float dmg = data.damages[idx];
            line += $"\n공격력 +{dmg:0.#}";
        }

        // 필요하면 투사체 개수/쿨타임 등도 비슷하게 이어서 추가
        // if (data.projectileCounts != null) { ... }

        return line;
    }

    string BuildGearDesc(GearData data, int nextLv1Based)
    {
        string line = !string.IsNullOrEmpty(data.gearDesc) ? data.gearDesc : "능력 향상";

        if (data.Values != null && data.Values.Length > 0)
        {
            int idx = Mathf.Clamp(nextLv1Based - 1, 0, data.Values.Length - 1);
            float val = data.Values[idx];
            line += $"\n효과 +{val:0.#}";
        }
        return line;
    }

    // 선택 시 처리 (기존 로직 유지)
    public void OnClick()
    {
        var player = GameManager.instance.player;

        switch (itemCategory)
        {
            case ItemCategory.Weapon:
                {
                    var w = player.FindEquippedWeapon(weaponData);
                    if (w == null) player.EquipWeapon(weaponData);
                    else w.LevelUp();
                    break;
                }
            case ItemCategory.Gear:
                {
                    if (gearData.gearType == GearData.GearType.Heal)
                    {
                        player.Heal();
                    }
                    else
                    {
                        var g = player.FindEquippedGear(gearData);
                        if (g == null) player.EquipGear(gearData);
                        else g.LevelUp();
                    }
                    break;
                }
        }

        GameManager.instance.uiLevelUo.hide();
    }
}
