using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "펫/스킬/고양이 (아이템 회수 + 공격력 버프)", fileName = "CatSkillData")]
public class CatSkillData : PetSkillDefinition
{
    [Header("회수 설정")]
    [Tooltip("등급별 회수 범위 [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 회수 범위 [N,R,E,U,L]")]
    public float[] recallRange = new float[5] { 0, 0, 60, 70, 80 };

    [Tooltip("자석 펄스를 유지할 시간(초). 이 시간 동안 스캐너 반경을 크게 키움")]
    [InspectorName("회수 펄스 지속(초)")]
    public float recallPulseSeconds = 0.35f;

    [Tooltip("폴백 모드 사용 시, 오버랩으로 직접 끌어올 아이템 레이어 마스크")]
    [InspectorName("폴백 오버랩 레이어(선택)")]
    public LayerMask fallbackItemMask;

    [Header("공격력 버프")]
    [Tooltip("등급별 공격력 배수 (1.5 = 50% 증가)")]
    [InspectorName("등급별 공격력 배수 [N,R,E,U,L]")]
    public float[] atkMultiplier = new float[5] { 1f, 1f, 1.5f, 1.5f, 1.5f };

    [Tooltip("등급별 공격력 버프 지속(초) [일반, 레어, 에픽, 유니크, 레전드]")]
    [InspectorName("등급별 버프 지속(초) [N,R,E,U,L]")]
    public float[] atkDuration = new float[5] { 0, 0, 5, 5, 5 };

    public override IEnumerator Execute(PetSkillRunner ctx)
    {
        int i = ctx.rarity.Index();
        float range = recallRange[Mathf.Clamp(i, 0, recallRange.Length - 1)];

        // 1) ItemScanner 펄스(있으면 우선 사용)
        bool handled = false;
        if (ctx.player)
        {
            var scanner = ctx.player.GetComponentInChildren<ItemScanner>();
            if (scanner)
            {
                float prev = scanner.scanRange;
                scanner.scanRange = Mathf.Max(prev, range);

                float t = 0f;
                while (t < recallPulseSeconds) { t += Time.fixedDeltaTime; yield return new WaitForFixedUpdate(); }

                scanner.scanRange = prev;
                handled = true;
            }
        }

        // 2) 폴백: 스캐너가 없으면 오버랩으로 직접 끌어오기
        if (!handled && ctx.player && range > 0f)
        {
            var cols = Physics2D.OverlapCircleAll(ctx.player.position, range, fallbackItemMask);
            foreach (var c in cols)
            {
                if (!c || !c.gameObject.activeInHierarchy) continue;
                c.SendMessage("Collect", ctx.player, SendMessageOptions.DontRequireReceiver);
                c.SendMessage("Pickup", ctx.player, SendMessageOptions.DontRequireReceiver);
                c.SendMessage("OnPickup", ctx.player, SendMessageOptions.DontRequireReceiver);
                c.transform.position = ctx.player.position;
            }
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
