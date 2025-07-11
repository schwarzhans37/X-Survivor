using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PetData
{
    public int Id;                 // ���� ��ȣ
    public string PetType;         // Ex) ������, �����
    public string Grade;           // Ex) ���, ����, ����, ����ũ, ������
    public bool HasSkill;          // ��ų ���� ����
    public string Buff;            // ����
    public string Skill;           // ��ų 
    public int SkillCount;         // �ִ� ��ų Ƚ��
}
