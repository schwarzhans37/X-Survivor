using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PetSkillRunner : MonoBehaviour
{
    [Header("Bind")]
    public PetSkillDefinition data;
    public PetRarity rarity = PetRarity.Epic;

    [Tooltip("런타임에서 자동으로 채움")]
    public Transform player;
    public PetController pet;

    [Header("Trigger Settings")]
    public bool manualTrigger = true; // 수동 발동 여부
    // public bool autoLoop = false; // 자동 발동 여부

    private float cooldownRemain;

    void Awake()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
        if (!pet)
        {
            pet = GetComponentInParent<PetController>();
            if (!pet) pet = FindObjectOfType<PetController>();
        }
    }

    void Update()
    {
        if (!data) return;

        // 살아있을 때만 쿨 감소 (죽으면 멈춤, 부활하면 이어감)
        if (cooldownRemain > 0f && (pet == null || pet.IsAlive))
            cooldownRemain -= Time.deltaTime;

        // 2. manualTrigger가 true일 때만 키 입력을 받도록 수정합니다.
        if (manualTrigger && Input.GetKeyDown(data.triggerKey))
        {
            TryCast();
        }

        // (추가) 만약 자동 발동 기능도 필요하다면 아래 코드를 활성화 할 수 있습니다.
        // if (autoLoop)
        // {
        //     TryCast(); 
        // }
    }

    public bool TryCast()
    {
        if (!data) return false;
        if (cooldownRemain > 0f) return false;
        if (pet && !pet.IsAlive) return false;
        if (!data.CanUse(rarity)) return false;

        StartCoroutine(data.Execute(this));
        cooldownRemain = data.CooldownFor(rarity);
        return true;
    }

    public float CooldownRemaining => Mathf.Max(0f, cooldownRemain);
    public float CooldownNormalized
        => Mathf.Approximately(data?.CooldownFor(rarity) ?? 0f, 0f)
           ? 0f : Mathf.Clamp01(cooldownRemain / data.CooldownFor(rarity));
}
