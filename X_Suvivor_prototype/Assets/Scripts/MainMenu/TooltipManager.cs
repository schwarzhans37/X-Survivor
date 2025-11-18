using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요

public class TooltipManager : MonoBehaviour
{
    // 어디서든 쉽게 접근할 수 있도록 싱글톤으로 만듭니다.
    public static TooltipManager instance;

    [Header("UI 연결")]
    [Tooltip("툴팁 전체 패널 (배경 이미지 포함)")]
    public GameObject tooltipPanel;

    [Tooltip("내용을 표시할 텍스트 컴포넌트")]
    public TextMeshProUGUI tooltipText;
    // 만약 레거시 Text를 쓰신다면 위 줄을 public UnityEngine.UI.Text tooltipText; 로 바꾸세요.

    [Header("설정")]
    [Tooltip("마우스 커서로부터 얼마나 떨어져서 표시될지 설정")]
    public Vector2 offset = new Vector2(15, -15);

    private RectTransform panelRect;

    void Awake()
    {
        if (instance == null) instance = this;

        if (tooltipPanel != null)
        {
            panelRect = tooltipPanel.GetComponent<RectTransform>();
        }
    }

    void Start()
    {
        // 시작할 때는 툴팁을 숨깁니다.
        HideTooltip();
    }

    void Update()
    {
        // 툴팁 패널이 켜져 있을 때만 마우스를 따라다니게 합니다.
        if (tooltipPanel.activeSelf)
        {
            // 현재 마우스 위치를 가져옵니다.
            Vector2 mousePos = Input.mousePosition;

            // 마우스 위치에 오프셋을 더해서 툴팁 위치를 잡습니다.
            // (RectTransform의 Pivot이 (0, 1) 좌상단이어야 자연스럽습니다)
            tooltipPanel.transform.position = mousePos + offset;
        }
    }

    // 툴팁을 보여주는 함수
    public void ShowTooltip(string content)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        // 텍스트 내용을 채웁니다.
        tooltipText.text = content;

        // 패널을 켭니다.
        tooltipPanel.SetActive(true);

        // 켜지는 순간 위치를 한 번 업데이트 해줍니다.
        tooltipPanel.transform.position = (Vector2)Input.mousePosition + offset;
    }

    // 툴팁을 숨기는 함수
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}