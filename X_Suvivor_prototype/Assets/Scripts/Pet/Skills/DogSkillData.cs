using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Pet/Skill/Dog (AoE Stun + ATK)", fileName = "DogSkillData")]
public class DogSkillData : PetSkillDefinition
{
    [Header("스턴")]
    [Tooltip("등급별 스턴 지속(초) [N,R,E,U,L]")]
    [InspectorName("등급별 스턴(초) [N,R,E,U,L]")]
    public float[] stunDuration = new float[5] { 0f, 0f, 5f, 5f, 5f };

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

        // 1. 전장 전체 스턴 로직
        float stunSec = stunDuration[Mathf.Clamp(i, 0, stunDuration.Length - 1)];
        if (stunSec > 0f)
        {
            // 씬에 있는 모든 Enemy 컴포넌트를 찾습니다.
            var enemies = Object.FindObjectsOfType<Enemy>(false);
            for (int k = 0; k < enemies.Length; k++)
            {
                // 살아있는 적에게만 스턴을 적용합니다.
                if (enemies[k] != null && enemies[k].IsAlive)
                {
                    enemies[k].ApplyStun(stunSec);
                }
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