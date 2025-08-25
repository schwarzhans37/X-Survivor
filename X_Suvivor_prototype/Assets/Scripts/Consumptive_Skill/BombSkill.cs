using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombSkill : MonoBehaviour
{
    [Header("쿨타임")]
    public float cooldown = 6f;
    private float cooldownTimer = 0f;

    private Player player; // Player 스크립트 참조
    public float CooldownTimer => cooldownTimer;
    public float MaxCooldown => cooldown;
    public float CooldownRatio => Mathf.Clamp01(cooldownTimer / Mathf.Max(0.0001f, cooldown));

    public System.Action<float> OnCooldownStarted; // 쿨다운 시작 알림(초 단위)

    [Header("지뢰 설정")]
    public GameObject minePrefab;
    public int maxActiveMines = 3;
    public float placeOffset = 0.1f;
    public float armingDelay = 0.15f;

    private readonly List<LandMine> activeMines = new List<LandMine>();

    void Awake()
    {
        // 같은 게임 오브젝트에 있는 Player 컴포넌트를 가져옴
        player = GetComponent<Player>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        for (int i = activeMines.Count - 1; i >= 0; i--)
            if (activeMines[i] == null) activeMines.RemoveAt(i);
    }

    // Player Input 컴포넌트가 호출할 함수
    public void OnBombSkill(InputValue value)
    {
        if (cooldownTimer > 0f) return;
        StartCoroutine(PlaceMineCoroutine());
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
            // 설치 직후 아밍 지연(자기발동 방지)
            mine.Arm(armingDelay);
            activeMines.Add(mine);
            mine.OnExploded += () => activeMines.Remove(mine);
        }

        // (선택) 설치 연출 때문에 잠깐 대기하고 싶으면
        yield return null;
    }
}
