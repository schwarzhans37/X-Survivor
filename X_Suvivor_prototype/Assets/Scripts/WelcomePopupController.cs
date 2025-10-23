using UnityEngine;
using UnityEngine.UI;

public class WelcomePopupController : MonoBehaviour
{
    [Header("보상 설정")]
    [Tooltip("환영 선물로 지급할 골드")]
    public int welcomeGold = 100;

    [Header("UI 연결 (에디터에서 연결)")]
    public Button dogButton;    // 강아지 선택 버튼
    public Button catButton;    // 고양이 선택 버튼
    public Button rabbitButton; // 토끼 선택 버튼
    public Button CapybaraButton; // 카피바라 선택 버튼
    public Button PandaButton; // 판다 선택 버튼
    public Button confirmButton; // 확인 버튼

    // 선택한 펫의 ID를 임시 저장하는 변수
    private int selectedPetId = -1;

    void Start()
    {
        // 각 펫 버튼을 눌렀을 때 어떤 함수가 실행될지 지정합니다.
        // () => SelectPet(ID)는 "버튼이 눌리면 SelectPet 함수를 ID 값과 함께 실행해라"는 의미입니다.
        // ※ 펫 ID는 PetDatabase에 설정된 값과 정확히 일치해야 합니다.
        dogButton.onClick.AddListener(() => SelectPet(100)); // 강아지 ID
        catButton.onClick.AddListener(() => SelectPet(200)); // 고양이 ID 
        rabbitButton.onClick.AddListener(() => SelectPet(300)); // 토끼 ID
        CapybaraButton.onClick.AddListener(() => SelectPet(400)); // 카피바라 ID
        PandaButton.onClick.AddListener(() => SelectPet(500)); // 판다 ID

        // 확인 버튼을 눌렀을 때의 동작을 지정합니다.
        confirmButton.onClick.AddListener(ConfirmSelection);

        // 처음에는 펫을 선택하지 않았으므로 확인 버튼을 비활성화합니다.
        confirmButton.interactable = false;
    }

    // 펫 버튼 중 하나를 눌렀을 때 호출되는 함수
    private void SelectPet(int petId)
    {
        selectedPetId = petId;
        Debug.Log($"펫 ID: {petId} 선택됨");

        // TODO: 선택한 버튼의 테두리 색을 바꾸거나 크기를 키워 선택했음을 시각적으로 표시하면 좋습니다.

        // 펫을 선택했으므로 확인 버튼을 누를 수 있게 활성화합니다.
        confirmButton.interactable = true;
    }

    // 확인 버튼을 눌렀을 때 호출되는 함수
    private void ConfirmSelection()
    {
        // 펫이 선택되지 않았다면 아무것도 하지 않습니다. (안전장치)
        if (selectedPetId == -1)
        {
            Debug.LogWarning("선택된 펫이 없습니다!");
            return;
        }

        // PlayerDataManager의 인스턴스를 가져옵니다.
        var pdm = PlayerDataManager.instance;

        // 1. 골드를 지급합니다.
        pdm.AddGold(welcomeGold);

        // 2. 선택한 펫을 해금합니다. (ownedPetIds 리스트에 추가)
        pdm.UnlockPet(selectedPetId);

        // 3. 변경된 모든 데이터를 파일에 저장합니다.
        pdm.SaveData();

        // 4. 모든 작업이 끝났으므로 팝업창을 닫습니다.
        gameObject.SetActive(false);
    }
}
