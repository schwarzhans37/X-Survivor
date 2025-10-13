using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "펫/스킬/토끼 (이속 + 임시 하트 + 공격력)", fileName = "RabbitSkillData")]
public class RabbitSkillData : PetSkillDefinition
{
    [Header("이동속도 버프")]
    [Tooltip("등급별 이동속도 배수(1.5 = 50% 증가) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 속도 배수 [N,R,E,U,L]")]
    public float[] speedMul = new float[5] { 0f, 0f, 1.5f, 1.5f, 1.5f };

    [Tooltip("등급별 속도 버프 지속(초) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 속도 지속(초) [N,R,E,U,L]")]
    public float[] speedDur = new float[5] { 0f, 0f, 5f, 5f, 5f };

    [Header("임시 최대체력")]
    [Tooltip("등급별 임시 하트 개수(+칸) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 임시 하트 [N,R,E,U,L]")]
    public int[] tempMaxHp = new int[5] { 0, 0, 1, 1, 1 };

    [Tooltip("등급별 임시 하트 지속(초) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 임시하트 지속(초) [N,R,E,U,L]")]
    public float[] tempHpDur = new float[5] { 0f, 0f, 20f, 25f, 30f };

    [Tooltip("임시 하트가 생길 때 해당 분량만큼 즉시 회복할지 여부")]
    public bool healNewHeart = true;

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

        // 임시 하트
        if (ctx.player)
        {
            int hearts = Mathf.Clamp(tempMaxHp[Mathf.Clamp(i, 0, tempMaxHp.Length - 1)], 0, 999);
            float hpDur = tempHpDur[Mathf.Clamp(i, 0, tempHpDur.Length - 1)];
            if (hearts > 0 && hpDur > 0f)
            {
                var thp = ctx.player.GetComponent<PlayerTempMaxHealthBuffReceiver>();
                if (!thp) thp = ctx.player.gameObject.AddComponent<PlayerTempMaxHealthBuffReceiver>();
                thp.AddTemporary(hearts, hpDur, healNewHeart);
            }
        }

        // 이동속도
        {
            float mul = speedMul[Mathf.Clamp(i, 0, speedMul.Length - 1)];
            float dur = speedDur[Mathf.Clamp(i, 0, speedDur.Length - 1)];
            if (ctx.player && mul > 0f && dur > 0f)
            {
                var sp = ctx.player.GetComponent<PlayerMoveSpeedBuffReceiver>();
                if (!sp) sp = ctx.player.gameObject.AddComponent<PlayerMoveSpeedBuffReceiver>();
                sp.AddMultiplier(mul, dur);
            }
        }

        // 공격력 버프
        {
            float bonus = atkFlat[Mathf.Clamp(i, 0, atkFlat.Length - 1)];
            float dur = atkDuration[Mathf.Clamp(i, 0, atkDuration.Length - 1)];
            if (ctx.player && dur > 0f && Mathf.Abs(bonus) > 0.0001f)
            {
                var atk = ctx.player.GetComponent<PlayerAttackBuffReceiver>();
                if (!atk) atk = ctx.player.gameObject.AddComponent<PlayerAttackBuffReceiver>();
                atk.AddFlatAttackBuff(bonus, dur);
            }
        }

        yield return null;
    }
}
