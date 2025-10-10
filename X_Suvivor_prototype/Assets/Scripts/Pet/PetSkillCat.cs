using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PetSkillCat : PetSkill
{
    [Header("Cat: Recall via ItemScanner")]
    [Tooltip("전장 펄스(스캔 반경). 씬 크기보다 크게 설정")]
    public float recallRange = 50f;

    [Tooltip("스캔 반경을 확장할 시간(물리 프레임 몇 번 줄지)")]
    public float recallPulseSeconds = 0.25f;

    [Tooltip("ItemScanner가 없거나 작동 안 할 때, 강제로 끌어오는 폴백 사용")]
    public bool useFallbackAttract = true;

    [Header("Cat: Player Attack Buff")]
    public float flatAttackBonus = 30f;
    [Min(0)] public float attackBuffDuration = 5f;

    protected override IEnumerator Cast()
    {
        // 1) 아이템 회수 — 스캐너 펄스
        yield return StartCoroutine(RecallWithItemScanner());

        // 2) 5초 공격력 +30
        if (player && attackBuffDuration > 0f && Mathf.Abs(flatAttackBonus) > 0.0001f)
        {
            var atk = player.GetComponent<PlayerAttackBuffReceiver>();
            if (!atk) atk = player.gameObject.AddComponent<PlayerAttackBuffReceiver>();
            atk.AddFlatAttackBuff(flatAttackBonus, attackBuffDuration);
        }
    }

    private IEnumerator RecallWithItemScanner()
    {
        if (!player) yield break;

        // 플레이어(또는 자석 오브젝트)에 붙은 스캐너 찾기
        var scanner = player.GetComponentInChildren<ItemScanner>();
        if (scanner)
        {
            float prev = scanner.scanRange;
            // 전장 전체를 덮도록 확장
            scanner.scanRange = Mathf.Max(prev, recallRange);

            // 물리 업데이트 몇 번 돌리는 동안 자석 로직이 Targets를 빨아가도록 대기
            float t = 0f;
            while (t < recallPulseSeconds)
            {
                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            // 원래 값 복원
            scanner.scanRange = prev;
            yield break;
        }

        // 스캐너가 없으면 폴백: 해당 레이어의 아이템을 직접 끌어오기/텔레포트
        if (useFallbackAttract)
        {
            // 보편적 폴백: EXP/Drop 레이어가 있다면 그 레이어 마스크로 교체
            int mask = LayerMask.GetMask("Exp", "Drop", "Item");
            var cols = Physics2D.OverlapCircleAll(player.position, recallRange, mask);
            for (int i = 0; i < cols.Length; i++)
            {
                var it = cols[i];
                if (!it || !it.gameObject.activeInHierarchy) continue;

                // 프로젝트에 있는 픽업 메서드가 있으면 먼저 호출
                it.SendMessage("Collect", player, SendMessageOptions.DontRequireReceiver);
                it.SendMessage("Pickup", player, SendMessageOptions.DontRequireReceiver);
                it.SendMessage("OnPickup", player, SendMessageOptions.DontRequireReceiver);

                // 그래도 안 먹히면 그냥 플레이어 위치로 이동
                it.transform.position = player.position;
            }
        }
    }
}
