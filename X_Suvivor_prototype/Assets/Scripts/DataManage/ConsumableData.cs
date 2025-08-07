using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = " Consumable", menuName = "ConsumableData")]
public class ConsumableData : ScriptableObject
{
    public enum ConsumableType { Heal }

    [Header("기본 정보")]
    public ConsumableType consumableType;
    public int consumableId;
    public string comsumableName;
    [TextArea]
    public string consumableDesc;
    public Sprite consumableIcon;
}
