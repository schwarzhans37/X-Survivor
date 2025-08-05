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

    // ==========================================================
    [Tooltip("이 무기의 행동 로직이 담긴 프리팹 (Melee, Projectile 등)")]
    public GameObject controllerPrefab;
    // ==========================================================

    [Header("# 레벨 데이터")]
    public float baseDamage;          // 레벨 0 (기본) 데미지
    public int baseCount;             // 레벨 0 (기본) 개수
    public int basePenetration;       // 레벨 0 (기본) 관통력
    public float baseCooldown;        // 레벨 0 (기본) 쿨다운
    public float baseProjectileSpeed; // 레벨 0 (기본) 투사체/회전 속도

    // 레벨업에 따른 '증가량'을 배열로 관리
    public float[] damages;
    public int[] counts;
    public int[] penetrations;
    public float[] cooldowns;
    public float[] projectileSpeeds;

    [Header("# 이미지 데이터")]
    public Sprite weaponIcon;         // UI 아이콘
    public GameObject projectilePrefab;     // 발사될 투사체 또는 회전할 칼날의 모양 프리팹
    public Sprite handSprite;         // 플레이어 손에 표시될 스프라이트
}
