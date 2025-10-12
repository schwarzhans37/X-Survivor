using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PetUpgradeType
{
    AttackDamage,
    AttackCooldown,
    ProjectileCount,
    MaxHealth
}

[CreateAssetMenu(fileName = "PetUpgrade_", menuName = "Pet/Pet Upgrade Data")]
public class PetUpgradeData : ScriptableObject
{
    [Header("강화 타입")]
    public PetUpgradeType upgradeType;

    [Header("UI 정보")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string upgradeDescription;
    public Sprite upgradeIcon;

    [Header("강화 수치")]
    public float[] upgradeValues; // 각 타입에 맞는 수치를 여기에 입력 (예: 공격력 +5, 쿨감 -0.15f)
}