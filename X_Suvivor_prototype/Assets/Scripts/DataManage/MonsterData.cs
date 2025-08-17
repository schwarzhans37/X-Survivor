using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "MonsterData", order = 1)]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public int Id;
    public string Name;
    public enum MonsterTier { Normal, Elite, Boss }
    public MonsterTier tier = MonsterTier.Normal;

    [Header("능력치")]
    public int Maxhealth;
    public int AttackPower;
    public int AttackRange;
    public float Speed;

    [Header("전용 리소스")]
    [Tooltip("이 데이터 해당하는 몬스터의 애니메이터 컨트롤러를 연결함")]
    public RuntimeAnimatorController animator;

    [Header("# 기타")]
    public int PoolManagerIndex;
}
