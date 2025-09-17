using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PetData_", menuName = "Pet/Pet Data")]
public class PetData : ScriptableObject
{
    [Header("기본 정보")]
    public int id;
    public string petName;
    public string petType;  // 예 : "강아지", "고양이"

    [Header("등급 정보")]
    public string grade;    // 예 : "노멀" , "레어", "유니크"

    [Header("그래픽 에셋")]
    public Sprite petIcon;       // UI에 표시될 아이콘
    public GameObject petSlot;
    public GameObject inGamePrefab;     // 게임 플레이 중 플레이어를 따라다닐 펫 프리팹
    public GameObject splashArtPrefab;  // 가챠 결과창에 표시될 스플래시 아트 프리팹



    [Header("능력 및 버프")]
    [TextArea(3, 5)]
    public string buffDescription;
    // 이곳에 실제로 적용될 버프 값 추가
    // 예: public float attackBonus;
    // 예: public float speedBonus;

    [Header("스킬 정보")]
    public bool hasSkill;
    [TextArea(3, 5)]
    // public Sprite skillIcon;     스킬 UI 아이콘
    public string skillDescription;
    public int initialSkillCount;
    // public PetSkill skillPrefab;   펫 전용 스킬 로직 프리팹 연결
}
