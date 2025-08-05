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
        name = "Gear" + data.gearName;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        //프로퍼티 설정
        type = data.gearType;
        rate = data.baseValue;
        ApplyGear();
    }

    public void LevelUp()
    {
        if (currentLevel >= gearData.Values.Length) return; // 최대 레벨

        currentLevel++;
        // 레벨에 맞는 최종값으로 rate를 업데이트
        rate = gearData.Values[currentLevel - 1];
        ApplyGear();
    }

    void ApplyGear()
    {
        switch (type)
        {
            case GearData.GearType.Glove:
                RateUp();
                break;
            case GearData.GearType.Shoe:
                SpeedUp();
                break;
        }
    }

    void RateUp()
    {
        // 플레이어의 모든 무기에게 "공격속도를 재계산해달라"는 메세지 보냄
        player.BroadcastMessage("ApplyGearRate", rate, SendMessageOptions.DontRequireReceiver);
        Debug.Log($"모든 무기의 공격속도가 {rate * 100}% 만큼 변경됩니다.");
    }

    void SpeedUp()
    {
        // 플레이어에게 속도 보너스를 적용하도록 요청
        player.UpdateSpeedBonus(rate);
        Debug.Log($"플레이어의 이동속도가 {rate * 100}% 만큼 증가합니다.");
    }
}
