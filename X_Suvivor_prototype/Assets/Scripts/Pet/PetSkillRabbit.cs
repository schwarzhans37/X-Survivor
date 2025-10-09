using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PetSkillRabbit : PetSkill
{
    [Header("Rabbit: Speed Buff (5s)")]
    [Tooltip("이동속도 배수(1.5 = 50% 증가)")]
    public float speedMultiplier = 1.5f;
    [Min(0)] public float speedBuffDuration = 5f;

    [Header("Rabbit: Temporary Max Health")]
    [Tooltip("임시 최대체력 증가량")]
    public int tempMaxHealth = 1;
    [Tooltip("임시 최대체력 지속시간(초) — 요구사항: 60초")]
    public float tempMaxHealthDuration = 20f;
    [Tooltip("늘어난 하트만큼 즉시 회복")]
    public bool healNewHeart = true;

    [Header("Rabbit: Player Attack Buff (5s)")]
    public float flatAttackBonus = 30f;
    [Min(0)] public float attackBuffDuration = 5f;

    protected override IEnumerator Cast()
    {
        // 1) 임시 최대체력 + 회복
        if (player && tempMaxHealth > 0 && tempMaxHealthDuration > 0f)
        {
            var thp = player.GetComponent<PlayerTempMaxHealthBuffReceiver>();
            if (!thp) thp = player.gameObject.AddComponent<PlayerTempMaxHealthBuffReceiver>();
            thp.AddTemporary(tempMaxHealth, tempMaxHealthDuration, healNewHeart);
        }

        // 2) 이동속도 5초 버프
        if (player && speedBuffDuration > 0f && speedMultiplier > 0f)
        {
            var sp = player.GetComponent<PlayerMoveSpeedBuffReceiver>();
            if (!sp) sp = player.gameObject.AddComponent<PlayerMoveSpeedBuffReceiver>();
            sp.AddMultiplier(speedMultiplier, speedBuffDuration);
        }

        // 3) 5초 공격력 버프
        if (player && attackBuffDuration > 0f && Mathf.Abs(flatAttackBonus) > 0.0001f)
        {
            var atk = player.GetComponent<PlayerAttackBuffReceiver>();
            if (!atk) atk = player.gameObject.AddComponent<PlayerAttackBuffReceiver>();
            atk.AddFlatAttackBuff(flatAttackBonus, attackBuffDuration);
        }

        yield return null;
    }
}
