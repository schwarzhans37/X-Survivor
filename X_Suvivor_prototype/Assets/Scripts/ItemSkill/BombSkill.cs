using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombSkill : MonoBehaviour
{
    [Header("쿨타임")]
    public float cooldown = 6f;
    public float cooldownTimer = 0f;

    private Player player; // Player 스크립트 참조

    public System.Action<float> OnCooldownStarted; // 쿨다운 시작 알림(초 단위)

    [Header("지뢰 설정")]
    public GameObject minePrefab;
    public int baseDamage = 40;         // 지뢰의 기본 데미지
    public int damageBonusPerLevel = 5; // 레벨당 추가 데미지
    public int maxActiveMines = 3;
    public float placeOffset = 0.1f;
    public float armingDelay = 0.15f;

    private readonly List<LandMine> activeMines = new List<LandMine>();

    void Awake()
    {
        // SkillInventory가 이 스크립트를 플레이어의 자식으로 생성하기 때문에 Player를 찾는 로직은 부모 오브젝트에서 찾도록 변경하는 것이 더 안전
        player = GetComponentInParent<Player>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        for (int i = activeMines.Count - 1; i >= 0; i--)
            if (activeMines[i] == null) activeMines.RemoveAt(i);
    }

    // Player Input 컴포넌트가 호출할 함수
    public bool TryUse()
    {
        if (cooldownTimer > 0f)
        {
            return false; // 쿨타임 중이라 사용 실패
        }

        StartCoroutine(PlaceMineCoroutine());
        return true; // 사용 성공
    }

    public float GetCooldownRatio()
    {
        return Mathf.Clamp01(cooldownTimer / cooldown);
    }

    private IEnumerator PlaceMineCoroutine()
    {
        // 쿨 시작(바로)
        cooldownTimer = cooldown;

        // 발밑 오프셋
        Vector3 pos = transform.position + Vector3.down * placeOffset;

        // 프리팹 생성
        var go = Instantiate(minePrefab, pos, Quaternion.identity);
        var mine = go.GetComponent<LandMine>();
        if (mine != null)
        {
            int finalDamage = baseDamage + damageBonusPerLevel * (GameManager.instance.level - 1);
            mine.damage = finalDamage;

            // 설치 직후 아밍 지연(자기발동 방지)
            mine.Arm(armingDelay);
            activeMines.Add(mine);
            mine.OnExploded += () => activeMines.Remove(mine);
        }

        // (선택) 설치 연출 때문에 잠깐 대기하고 싶으면
        yield return null;
    }
}
