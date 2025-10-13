using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PetKind { Dog, Cat, Rabbit, Panda, Capybara }

[CreateAssetMenu(fileName = "PetData_", menuName = "Pet/Pet Data")]
public class PetData : ScriptableObject
{
    [Header("기본 정보")]
    public int id;
    public string petName;
    public PetKind petType;  // 예 : "강아지", "고양이"

    [Header("등급 정보")]
    public PetRarity grade;    // 예 : "노멀" , "레어", "유니크"

    [Header("그래픽 에셋")]
    public Sprite petIcon;       // UI에 표시될 아이콘
    public GameObject inGamePrefab;     // 게임 플레이 중 플레이어를 따라다닐 펫 프리팹
    public GameObject splashArtPrefab;  // 가챠 결과창에 표시될 스플래시 아트 프리팹



    [Header("능력 및 버프")]
    [TextArea(3, 5)]
    public string buffDescription;
    // 이곳에 실제로 적용될 버프 값 추가
    // 예: public float attackBonus;
    // 예: public float speedBonus;

    [Header("스킬 연결")]
    [Tooltip("이 종(species)의 스킬 SO. (예: DogSkillData, CatSkillData, RabbitSkillData)")]
    public PetSkillDefinition skillData; // 에픽 미만이어도 넣어두면 됨. 등급 게이트로 막힘.

    // 편의 프로퍼티
    public bool HasSkill => grade >= PetRarity.Epic && skillData != null;
}
