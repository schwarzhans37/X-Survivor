using System.Collections;
using UnityEngine;

public abstract class PetSkillDefinition : ScriptableObject
{
    [Header("사용 조건 & 입력키")]
    [Tooltip("이 등급 이상만 사용 가능")]
    public PetRarity minRarityToUse = PetRarity.Epic;

    [Tooltip("수동 발동 키")]
    public KeyCode triggerKey = KeyCode.R;

    [Header("등급별 쿨타임(초) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 쿨타임(초) [N,R,E,U,L]")]
    public float[] cooldownByRarity = new float[5] { Mathf.Infinity, Mathf.Infinity, 45f, 35f, 20f };

    public bool CanUse(PetRarity rarity) => rarity >= minRarityToUse;

    public float CooldownFor(PetRarity rarity)
    {
        int i = rarity.Index();
        return (i >= 0 && i < cooldownByRarity.Length) ? cooldownByRarity[i] : Mathf.Infinity;
    }

    // 실제 실행 로직 (자식 SO에서 구현)
    public abstract IEnumerator Execute(PetSkillRunner ctx);
}
