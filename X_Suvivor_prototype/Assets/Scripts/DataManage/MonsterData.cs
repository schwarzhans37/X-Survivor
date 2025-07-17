using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterData
{
    public int Id;
    public string Name;
    public string Category;
    public int Maxhealth;
    public int AttackPower;
    public int AttackRange;
    public float Speed;
    public float Size;
    public bool isElite;
    public bool isBoss;
    public RuntimeAnimatorController animator;
}
