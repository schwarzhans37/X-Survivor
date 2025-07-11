using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PetData
{
    public int Id;                 // 고유 번호
    public string PetType;         // Ex) 강아지, 고양이
    public string Grade;           // Ex) 노멀, 레어, 에픽, 유니크, 레전드
    public bool HasSkill;          // 스킬 보유 여부
    public string Buff;            // 버프
    public string Skill;           // 스킬 
    public int SkillCount;         // 최대 스킬 횟수
}
