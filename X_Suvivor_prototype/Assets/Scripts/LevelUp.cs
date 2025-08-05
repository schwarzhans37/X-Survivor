using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    public List<WeaponData> weaponDatas; // 인스펙터에서 모든 무기 데이터 연결
    public List<GearData> gearDatas;   // 인스펙터에서 모든 장비 데이터 연결
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
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
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

        // 1. 선택 가능한 모든 아이템(무기+장비) 목록을 만듭니다.
        List<object> candidateItems = new List<object>();
        candidateItems.AddRange(weaponDatas);
        candidateItems.AddRange(gearDatas);

        // 2. 이 중에서 랜덤하게 3개를 중복 없이 뽑습니다.
        List<object> selectedItems = new List<object>();
        while (selectedItems.Count < 3 && candidateItems.Count > 0)
        {
            int randomIndex = Random.Range(0, candidateItems.Count);
            selectedItems.Add(candidateItems[randomIndex]);
            candidateItems.RemoveAt(randomIndex);
        }

        // 3. 뽑힌 아이템들을 UI 슬롯에 할당합니다.
        for (int i = 0; i < selectedItems.Count; i++)
        {
            object selectedData = selectedItems[i];
            Item uiSlot = items[i];

            if (selectedData is WeaponData) // 뽑힌 것이 무기 데이터라면
            {
                uiSlot.Init((WeaponData)selectedData);
            }
            else if (selectedData is GearData) // 뽑힌 것이 장비 데이터라면
            {
                uiSlot.Init((GearData)selectedData);
            }
            
            uiSlot.gameObject.SetActive(true);
        }
    }
}
