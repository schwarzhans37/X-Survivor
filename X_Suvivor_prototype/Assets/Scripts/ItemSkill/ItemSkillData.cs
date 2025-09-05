using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스킬 종류를 쉽게 구분하기 위한 열거형
public enum ItemSkillType { Lightning, Bomb, MagneticField, SplitArrows }

[CreateAssetMenu(fileName = "ItemSkillData", menuName = "ItemSkillData", order = 2)]
public class ItemSkillData : ScriptableObject
{
    [Header("기본 정보")]
    public ItemSkillType skillType;
    public string skillName;
    public Sprite skillIcon;

    [Header("게임 로직")]
    [Tooltip("플레이어에게 부착될 스킬 기능이 담긴 프리팹")]
    public GameObject skillPrefab;
}
