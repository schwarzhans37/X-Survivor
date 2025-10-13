using UnityEngine;

[DisallowMultipleComponent]
public class PetSkillInstaller : MonoBehaviour
{
    [Header("선택한 펫의 스킬 프리팹")]
    public GameObject skillPrefab;     // 예: DogSkillPrefab, CatSkillPrefab, RabbitSkillPrefab
    public PetRarity rarity = PetRarity.Epic;

    [Tooltip("플레이어 밑으로 달지 않으면, 이 오브젝트 밑으로 붙음")]
    public bool parentUnderPlayer = true;

    private PetSkillRunner runnerInstance;

    void Start()
    {
        if (!skillPrefab) return;

        Transform parent = transform;
        if (parentUnderPlayer)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo) parent = playerGo.transform;
        }

        var go = Instantiate(skillPrefab, parent);
        runnerInstance = go.GetComponentInChildren<PetSkillRunner>();
        if (!runnerInstance) runnerInstance = go.AddComponent<PetSkillRunner>();

        runnerInstance.rarity = rarity; // 등급 주입
        // data는 프리팹 내부 Runner에 이미 연결해두는 것을 권장
    }

    // 원하면 런타임에 등급 전환도 가능
    public void SetRarity(PetRarity r)
    {
        rarity = r;
        if (runnerInstance) runnerInstance.rarity = r;
    }
}
