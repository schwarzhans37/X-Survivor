using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharaData", menuName = "Scriptable Object/CharacterData")]
public class CharaData : ScriptableObject
{
    public int charaId;
    public string charaName;
    public WeaponData startingWeapon;     // 캐릭터 별 시작 무기
    //이곳에 나중에 캐릭터의 고유 스킬이나 기본 스탯 보너스 등을 추가할 수 있음
}
