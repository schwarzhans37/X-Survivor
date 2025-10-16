using System.Collections;
using UnityEngine;

// 파일 이름과 메뉴 이름을 새로운 기능에 맞게 수정했습니다.
[CreateAssetMenu(menuName = "펫/스킬/토끼 (이속 + 무적 + 공격력)", fileName = "RabbitSkillData")]
public class RabbitSkillData : PetSkillDefinition
{
    [Header("이동속도 버프")]
    [Tooltip("등급별 이동속도 배수(1.5 = 50% 증가) [N,R,E,U,L]")]
    [InspectorName("등급별 속도 배수 [N,R,E,U,L]")]
    public float[] speedMul = new float[5] { 1f, 1f, 1.5f, 1.5f, 1.5f };

    [Tooltip("등급별 속도 버프 지속(초) [N,R,E,U,L]")]
    [InspectorName("등급별 속도 지속(초) [N,R,E,U,L]")]
    public float[] speedDur = new float[5] { 0f, 0f, 3f, 3f, 3f };

    [Header("무적 효과")]
    [Tooltip("등급별 무적 지속 시간(초) [N,R,E,U,L]")]
    [InspectorName("등급별 무적 시간(초) [N,R,E,U,L]")]
    public float[] invincibilityDuration = new float[5] { 0f, 0f, 3f, 3f, 3f };

    [Header("공격력 버프")]
    [Tooltip("등급별 공격력 배수 (1.5 = 50% 증가)")]
    [InspectorName("등급별 공격력 배수 [N,R,E,U,L]")]
    public float[] atkMultiplier = new float[5] { 1f, 1f, 1.5f, 1.5f, 1.5f };

    [Tooltip("등급별 공격력 버프 지속(초) [N,R,E,U,L]")]
    [InspectorName("등급별 버프 지속(초) [N,R,E,U,L]")]
    public float[] atkDuration = new float[5] { 0f, 0f, 5f, 5f, 5f };

    public override IEnumerator Execute(PetSkillRunner ctx)
    {
        // 플레이어 정보가 없으면 아무것도 하지 않고 종료
        if (ctx.player == null)
        {
            yield break;
        }

        int i = ctx.rarity.Index();

        // 1. 이동속도 버프 로직
        float speedMulValue = speedMul[Mathf.Clamp(i, 0, speedMul.Length - 1)];
        float speedDurValue = speedDur[Mathf.Clamp(i, 0, speedDur.Length - 1)];
        if (speedMulValue > 1f && speedDurValue > 0f) // 배수가 1을 초과해야 의미가 있음
        {
            // 플레이어에게 PlayerMoveSpeedBuffReceiver 컴포넌트를 찾거나 없으면 추가
            var sp = ctx.player.GetComponent<PlayerMoveSpeedBuffReceiver>();
            if (!sp) sp = ctx.player.gameObject.AddComponent<PlayerMoveSpeedBuffReceiver>();
            sp.AddMultiplier(speedMulValue, speedDurValue);
        }

        // 2. 무적 효과 로직 (새롭게 변경된 부분)
        float invincibilityDurValue = invincibilityDuration[Mathf.Clamp(i, 0, invincibilityDuration.Length - 1)];
        if (invincibilityDurValue > 0f)
        {
            // 플레이어에게 PlayerInvincibilityBuffReceiver 컴포넌트를 찾거나 없으면 추가
            var inv = ctx.player.GetComponent<PlayerInvincibilityBuffReceiver>();
            if (!inv) inv = ctx.player.gameObject.AddComponent<PlayerInvincibilityBuffReceiver>();
            inv.GrantInvincibility(invincibilityDurValue);
        }

        // 3. 공격력 배율 버프 로직
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