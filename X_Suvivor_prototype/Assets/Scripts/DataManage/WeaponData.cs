using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "Weapon", menuName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    public enum WeaponType { Melee, Range, Magic };

    [Header("# 무기 정보")]
    public WeaponType weaponType;
    public int weaponId;
    public string weaponName;
    [TextArea]
    public string weaponDesc;


    [Header("# 레벨 데이터")]
    public float baseDamage;
    public int basePenetration; //  관통 횟수
    public int baseCount;       // 투사체 수(보통은 1, 산탄총 같은 무기 추가 시 이걸 활용할 것)
    public float baseCooldown;  // 사격 간 대기시간
    public float baseProjectileSpeed;   // 투사체 속도

    // 레벨업에 따른 '증가량'을 배열로 관리
    public float[] damages;
    public int[] penetrations;
    public int[] counts;
    public float[] cooldowns;

    [Header("# 이미지 데이터")]
    public Sprite weaponIcon;
    public GameObject projectile;
    public Sprite handSprite;
    public float bulletBaseSpeed;
}
