using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "펫/스킬/판다 (광역 피해)", fileName = "PandaSkillData")]
public class PandaSkillData : PetSkillDefinition
{
    [Header("펫 위치 기준 광역 피해 설정")]
    [Tooltip("피해를 줄 범위 (반지름)")]
    public float damageRadius = 10f;

    [Tooltip("피해량 (펫 공격력 기반 배율) [N,R,E,U,L]")]
    [InspectorName("등급별 피해 배율 [N,R,E,U,L]")]
    public float[] damageMultiplier = new float[5] { 0f, 0f, 5.0f, 6.0f, 7.0f };

    [Tooltip("광역 피해 이펙트 프리팹 (선택 사항)")]
    public GameObject areaEffectPrefab;

    [Header("공격력 버프")]
    [Tooltip("등급별 공격력 배수 (1.5 = 50% 증가)")]
    [InspectorName("등급별 공격력 배수 [N,R,E,U,L]")]
    public float[] atkMultiplier = new float[5] { 1f, 1f, 1.5f, 1.5f, 1.5f };

    [Tooltip("등급별 공격력 버프 지속(초)")]
    [InspectorName("등급별 버프 지속(초) [N,R,E,U,L]")]
    public float[] atkDuration = new float[5] { 0f, 0f, 5f, 5f, 5f };

    public override IEnumerator Execute(PetSkillRunner ctx)
    {
        // 펫이 없으면 스킬 발동 불가
        if (!ctx.pet) yield break;

        int i = ctx.rarity.Index();

        // 1. 광역 피해
        float currentDamageMultiplier = damageMultiplier[Mathf.Clamp(i, 0, damageMultiplier.Length - 1)];
        if (currentDamageMultiplier > 0)
        {
            // 이펙트가 있다면 펫의 위치에 생성
            if (areaEffectPrefab != null)
            {
                Instantiate(areaEffectPrefab, ctx.pet.transform.position, Quaternion.identity);
            }

            // 펫의 현재 공격력을 기반으로 최종 피해량 계산
            float finalDamage = ctx.pet.baseAttackDamage * currentDamageMultiplier;

            // 펫 주변의 damageRadius 반경 내 모든 적을 찾음
            Collider2D[] enemies = Physics2D.OverlapCircleAll(ctx.pet.transform.position, damageRadius, ctx.pet.enemyLayer);

            foreach (var enemyCollider in enemies)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.ApplyDamage(finalDamage, enemy.transform.position, 1.5f); // 약간 강한 넉백
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