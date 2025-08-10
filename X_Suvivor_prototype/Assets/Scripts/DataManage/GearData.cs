using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "Gear", menuName = "GearData")]
public class GearData : ScriptableObject
{
    public enum GearType { Glove, Shoe, Armor, Glass, Heal }

    [Header("기본 정보")]
    public GearType gearType;
    public int gearId;
    public string gearName;
    [TextArea]
    public string gearDesc;
    public Sprite gearIcon;

    [Header("레벨 데이터")]
    public float baseValue;
    public float[] Values;
}
