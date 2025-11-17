using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트를 감지하기 위해 필수!

public class InfoHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("표시할 내용")]
    [TextArea(3, 10)] // Inspector창에서 여러 줄을 입력할 수 있게 만듭니다.
    public string tooltipMessage = "여기에 설명을 입력하세요.";

    // 마우스가 이 오브젝트 위로 들어왔을 때 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.ShowTooltip(tooltipMessage);
        }
    }

    // 마우스가 이 오브젝트에서 나갔을 때 호출됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.HideTooltip();
        }
    }

    // (선택사항) 만약 버튼이 비활성화될 때 툴팁도 같이 꺼지게 하려면 추가
    void OnDisable()
    {
        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.HideTooltip();
        }
    }
}