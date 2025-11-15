using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "MonsterData", order = 1)]
public class MonsterData : ScriptableObject
{
    [Header("# PoolManager 인덱스")]
    public int PoolManagerIndex;

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

    [Header("드랍 아이템")]
    [Tooltip("최소 드랍할 경험치 구슬 갯수")]
    public int mixExpOrbs = 1;
    [Tooltip("최대 드랍할 경험치 구슬 개수")]
    public int maxExpOrbs = 1;
    [Tooltip("경험치 외 드랍 아이템")]
    public List<DropItem> dropList;

    [Header("전용 리소스")]
    [Tooltip("이 데이터 해당하는 몬스터의 애니메이터 컨트롤러를 연결함")]
    public RuntimeAnimatorController animator;
}

[System.Serializable]
public class DropItem
{
    [Tooltip("PoolManager의 Item 카테고리에 등록된 아이템 프리팹의 인덱스")]
    public int itemPoolIndex;
    [Tooltip("이 아이템이 드랍될 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float dropChance;
    [Tooltip("드랍될 최소 개수")]
    public int minAmount;
    [Tooltip("드랍될 최대 개수")]
    public int maxAmount;
}
