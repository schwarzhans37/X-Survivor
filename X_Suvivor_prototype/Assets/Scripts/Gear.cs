using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public GearData.GearType type;
    public GearData gearData;   // GearData 참조
    public float rate;
    public int currentLevel;
    protected Player player;

    void Awake()
    {
        player = GameManager.instance.player;
    }

    public void Init(GearData data)
    {
        this.gearData = data;   // 데이터 저장
        this.currentLevel = 0;  // 초기 레벨은 0

        // 기본 설정
        name = "장비 : " + data.gearName;

        //프로퍼티 설정
        type = data.gearType;
        rate = data.baseValue;
    }

    public void LevelUp()
    {
        if (currentLevel >= gearData.Values.Length) return; // 최대 레벨

        currentLevel++;
        // 레벨에 맞는 최종값으로 rate를 업데이트
        rate = gearData.Values[currentLevel - 1];

        GameManager.instance.player.UpdateAllStatsFromGears();
    }
}
