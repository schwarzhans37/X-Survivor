using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "펫/스킬/강아지 (전체 스턴 + 공격력)", fileName = "DogSkillData")]
public class DogSkillData : PetSkillDefinition
{
    [Header("스턴")]
    [Tooltip("등급별 스턴 지속(초) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 스턴(초) [N,R,E,U,L]")]
    public float[] stunDuration = new float[5] { 0f, 0f, 5f, 5f, 5f };

    [Header("공격력 버프")]
    [Tooltip("등급별 고정 공격력 보너스 [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 공격력 보너스 [N,R,E,U,L]")]
    public float[] atkFlat = new float[5] { 0f, 0f, 30f, 30f, 30f };

    [Tooltip("등급별 공격력 버프 지속(초) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 버프 지속(초) [N,R,E,U,L]")]
    public float[] atkDuration = new float[5] { 0f, 0f, 5f, 5f, 5f };

    public override IEnumerator Execute(PetSkillRunner ctx)
    {
        int i = ctx.rarity.Index();
        float stunSec = stunDuration[Mathf.Clamp(i, 0, stunDuration.Length - 1)];
        float bonus = atkFlat[Mathf.Clamp(i, 0, atkFlat.Length - 1)];
        float dur = atkDuration[Mathf.Clamp(i, 0, atkDuration.Length - 1)];

        // 전장 전체 스턴
        var enemies = Object.FindObjectsOfType<Enemy>(false);
        for (int k = 0; k < enemies.Length; k++)
            enemies[k].ApplyStun(stunSec);

        // 공격력 버프
        if (ctx.player && dur > 0f && Mathf.Abs(bonus) > 0.0001f)
        {
            var recv = ctx.player.GetComponent<PlayerAttackBuffReceiver>();
            if (!recv) recv = ctx.player.gameObject.AddComponent<PlayerAttackBuffReceiver>();
            recv.AddFlatAttackBuff(bonus, dur);
        }

        yield return null;
    }
}
