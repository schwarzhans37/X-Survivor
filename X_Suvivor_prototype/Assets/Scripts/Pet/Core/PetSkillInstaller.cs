using UnityEngine;
using System.Reflection;

[DisallowMultipleComponent]
public class PetSkillInstaller : MonoBehaviour
{
    [Header("자동 설치")]
    [Tooltip("메인메뉴에서 선택한 펫(PetData)로부터 등급/스킬 SO를 자동으로 읽어 설치")]
    public bool autoFromSelectedPet = true;

    [Header("수동 override (선택)")]
    [Tooltip("지정하면 이 프리팹을 인스턴스해서 사용 (내부에 PetSkillRunner가 있든지, 없으면 자동 추가)")]
    public GameObject skillPrefab;

    [Header("부모 설정")]
    [Tooltip("플레이어의 자식으로 붙일지 여부")]
    public bool parentUnderPlayer = true;

    private PetSkillRunner runnerInstance;

    // GameManager가 호출할 수 있도록 public 함수로 변경합니다.
    // petController 매개변수로 어떤 펫에게 스킬을 달아줄지 직접 받습니다.
    public void InstallSkillForPet(PetController petController)
    {
        // 부모 결정
        Transform parent = transform; // 기본 부모는 Installer 자기 자신
        if (parentUnderPlayer)
        {
            parent = GameManager.instance?.player?.transform
                  ?? GameObject.FindGameObjectWithTag("Player")?.transform
                  ?? transform;
        }

        if (autoFromSelectedPet)
        {
            InstallFromSelectedPet(parent, petController);
        }
        else
        {
            // 수동 프리팹 모드... (이 부분은 현재 로직과 맞지 않으므로 autoFromSelectedPet = true를 권장)
        }
    }

    void InstallFromSelectedPet(Transform parent, PetController petController)
    {
        var pdm = PlayerDataManager.instance;
        if (pdm == null || pdm.playerData == null) return;

        int petId = pdm.playerData.selectedPetId;
        if (petId <= 0) return;

        var petDB = pdm.petDatabase;
        var petData = petDB ? petDB.GetPetByID(petId) : null;
        if (petData == null) return;

        // 에픽 미만이거나 스킬 SO 없음 → 설치 안 함
        if (petData.grade < PetRarity.Epic || petData.skillData == null) return;

        GameObject go;
        if (skillPrefab != null)
        {
            go = Instantiate(skillPrefab, parent);
            runnerInstance = go.GetComponentInChildren<PetSkillRunner>() ?? go.AddComponent<PetSkillRunner>();
        }
        else
        {
            go = new GameObject($"{petData.petName}_SkillRunner");
            go.transform.SetParent(parent, false);
            runnerInstance = go.AddComponent<PetSkillRunner>();
        }

        // 선택된 펫 데이터 주입
        runnerInstance.data = petData.skillData;
        runnerInstance.rarity = petData.grade;
        runnerInstance.manualTrigger = true; // R키 수동 발동

        // (옵션) 플레이어/펫 참조 주입
        TryInjectField(runnerInstance, "player", parent);
        // 이제 FindObjectOfType으로 찾을 필요 없이, GameManager가 넘겨준 petController를 직접 사용합니다.
        if (petController != null)
        {
            TryInjectField(runnerInstance, "pet", petController);
            Debug.Log($"{petController.name}에게 스킬 러너를 성공적으로 주입했습니다.");
        }
    }

    // (이하 나머지 코드는 기존과 동일...)
    void TryInjectField(object target, string fieldName, object value)
    {
        var f = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType.IsInstanceOfType(value))
            f.SetValue(target, value);
    }

    public void SetRarity(PetRarity r)
    {
        if (runnerInstance) runnerInstance.rarity = r;
    }
}