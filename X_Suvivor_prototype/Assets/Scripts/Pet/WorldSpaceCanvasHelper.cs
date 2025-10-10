using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCanvasHelper : MonoBehaviour
{
    void Start()
    {
        // 1. 자기 자신에게 붙어있는 Canvas 컴포넌트를 찾습니다.
        Canvas canvas = GetComponent<Canvas>();

        // 2. 현재 씬에서 'MainCamera' 태그가 붙은 메인 카메라를 자동으로 찾아옵니다.
        if (Camera.main != null)
        {
            // 3. 찾아온 카메라를 Canvas의 이벤트 카메라(worldCamera)로 설정합니다.
            canvas.worldCamera = Camera.main;
        }
        else
        {
            Debug.LogError("씬에 'MainCamera' 태그가 붙은 카메라가 없습니다! Event Camera를 설정할 수 없습니다.");
        }
    }
}