using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PetSkillDog : PetSkill
{
    [Header("Dog Skill: Global Stun")]
    [Min(0)] public float stunDuration = 5f;

    [Header("Dog Skill: Player Attack Buff")]
    public float flatAttackBonus = 30f;
    [Min(0)] public float attackBuffDuration = 5f;

    [Header("Targeting")]
    public string enemyTag = "Enemy";

    // PetSkillDog.cs

    protected override IEnumerator Cast()
    {
        StunAllEnemies(stunDuration);

        if (player && attackBuffDuration > 0f && Mathf.Abs(flatAttackBonus) > 0.0001f)
        {
            var buff = player.GetComponent<PlayerAttackBuffReceiver>();
            if (!buff) buff = player.gameObject.AddComponent<PlayerAttackBuffReceiver>();
            buff.AddFlatAttackBuff(flatAttackBonus, attackBuffDuration);
        }
        yield return null;
    }

    private void StunAllEnemies(float duration)
    {
        var enemies = Object.FindObjectsOfType<Enemy>(false); // 활성만
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (!e || !e.isActiveAndEnabled) continue;

            // 네 프로젝트에 맞춰 IsAlive 또는 isLive 사용
            // if (e.IsAlive) e.ApplyStun(duration);
            e.ApplyStun(duration);
        }
    }
}

