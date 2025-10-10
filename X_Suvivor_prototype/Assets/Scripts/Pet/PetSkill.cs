using UnityEngine;
using System.Collections;

public enum PetRarity { Normal, Rare, Epic, Unique, Legendary }

// 수동(R키) 전용 베이스
public abstract class PetSkill : MonoBehaviour
{
    [Header("Pet Info")]
    [SerializeField] protected PetRarity rarity = PetRarity.Epic;

    [Header("Skill Gate")]
    [SerializeField] protected PetRarity minRarityToUse = PetRarity.Epic; // 에픽 이상만 사용

    [Header("Cooldown By Rarity (sec)")]
    [Min(0)] [SerializeField] protected float cooldownEpic = 45f;
    [Min(0)] [SerializeField] protected float cooldownUnique = 35f;
    [Min(0)] [SerializeField] protected float cooldownLegendary = 20f;

    [Header("Manual Trigger")]
    [SerializeField] protected KeyCode triggerKey = KeyCode.R; // 플레이어가 누를 키

    protected Transform player;
    protected PetController pet;        // 생존 여부 확인용
    protected float cooldownRemain = 0; // 남은 쿨(초)

    protected virtual void Awake()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) player = go.transform;
        pet = GetComponent<PetController>(); // 같은 오브젝트에 있다고 가정
    }

    // 자동 캐스트 없음
    protected virtual void OnEnable() { /* no-op */ }
    protected virtual void OnDisable() { StopAllCoroutines(); }

    protected virtual void Update()
    {
        // 살아있을 때만 쿨다운 감소 → 죽으면 멈춤, 부활하면 이어감
        if (cooldownRemain > 0f && (pet == null || pet.IsAlive))
            cooldownRemain -= Time.deltaTime;

        // R키 수동 발동
        if (Input.GetKeyDown(triggerKey))
            TryCast();
    }

    protected bool CanUseSkill() => rarity >= minRarityToUse;

    protected float GetCooldown()
    {
        switch (rarity)
        {
            case PetRarity.Legendary: return cooldownLegendary;
            case PetRarity.Unique: return cooldownUnique;
            case PetRarity.Epic: return cooldownEpic;
            default: return Mathf.Infinity; // 사용불가 등급
        }
    }

    /// <summary>키 입력/버튼/UI에서 호출. 성공 시 true.</summary>
    public bool TryCast()
    {
        if (!CanUseSkill()) return false; // 등급 미달
        if (cooldownRemain > 0f) return false; // 쿨 중
        if (pet != null && !pet.IsAlive) return false; // 죽어 있음

        StartCoroutine(Cast());
        cooldownRemain = GetCooldown();
        return true;
    }

    // 실제 스킬 효과(자식에서 구현)
    protected abstract IEnumerator Cast();

    // UI 표시용
    public float CooldownRemaining => Mathf.Max(0f, cooldownRemain);
    public float CooldownNormalized => Mathf.Approximately(GetCooldown(), 0f)
                                       ? 0f
                                       : Mathf.Clamp01(cooldownRemain / GetCooldown());
}
