using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "펫/스킬/카피바라 (보호막)", fileName = "CapybaraSkillData")]
public class CapybaraSkillData : PetSkillDefinition
{
    [Header("임시 보호막 (체력)")]
    [Tooltip("등급별 임시 하트 개수(+칸)")]
    [InspectorName("등급별 임시 하트 [N,R,E,U,L]")]
    public int[] tempMaxHp = new int[5] { 0, 0, 1, 1, 1 };

    [Tooltip("등급별 임시 하트 지속(초)")]
    [InspectorName("등급별 임시하트 지속(초) [N,R,E,U,L]")]
    public float[] tempHpDur = new float[5] { 0f, 0f, 10f, 10f, 10f };

    [Tooltip("임시 하트가 생길 때 해당 분량만큼 즉시 회복할지 여부")]
    public bool healNewHeart = true;

    [Header("공격력 버프")]
    [Tooltip("등급별 공격력 배수 (1.5 = 50% 증가)")]
    [InspectorName("등급별 공격력 배수 [N,R,E,U,L]")]
    public float[] atkMultiplier = new float[5] { 1f, 1f, 1.5f, 1.5f, 1.5f };

    [Tooltip("등급별 공격력 버프 지속(초)")]
    [InspectorName("등급별 버프 지속(초) [N,R,E,U,L]")]
    public float[] atkDuration = new float[5] { 0f, 0f, 5f, 5f, 5f };

    public override IEnumerator Execute(PetSkillRunner ctx)
    {
        int i = ctx.rarity.Index();

        // 1. 임시 체력 (보호막)
        if (ctx.player)
        {
            int hearts = tempMaxHp[Mathf.Clamp(i, 0, tempMaxHp.Length - 1)];
            float hpDur = tempHpDur[Mathf.Clamp(i, 0, tempHpDur.Length - 1)];
            if (hearts > 0 && hpDur > 0f)
            {
                var thp = ctx.player.GetComponent<PlayerTempMaxHealthBuffReceiver>();
                if (!thp) thp = ctx.player.gameObject.AddComponent<PlayerTempMaxHealthBuffReceiver>();
                thp.AddTemporary(hearts, hpDur, healNewHeart);
            }
        }

        // 2. 공격력 배율 버프 로직
        float multiplier = atkMultiplier[Mathf.Clamp(i, 0, atkMultiplier.Length - 1)];
        float dur = atkDuration[Mathf.Clamp(i, 0, atkDuration.Length - 1)];

        // 플레이어가 존재하고, 버프 지속시간이 있으며, 배율이 1배를 초과할 때만 실행
        if (ctx.player && dur > 0f && multiplier > 1f)
        {
            var recv = ctx.player.GetComponent<PlayerAttackBuffReceiver>();
            if (!recv) recv = ctx.player.gameObject.AddComponent<PlayerAttackBuffReceiver>();

            // AddFlatAttackBuff 대신 AddMultiplier를 호출합니다.
            recv.AddMultiplier(multiplier, dur);
        }

        yield return null;
    }
}