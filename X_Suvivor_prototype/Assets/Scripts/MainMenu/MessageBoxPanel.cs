using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MessageBoxPanel : MonoBehaviour, IPointerClickHandler
{
    // 이 UI 요소가 클릭되었을 때 자동으로 호출
    public void OnPointerClick(PointerEventData eventData)
    {
        // 자기 자신(이 스크립트가 붙어있는 오브젝트)를 비활성화
        gameObject.SetActive(false);
        Debug.Log("메시지 박스가 닫힙니다.");
    }

    // 외부에서 이 메세지 박스를 띄울 때 사용하는 함수
    public void Show()
    {
        gameObject.SetActive(true);
    }
}
