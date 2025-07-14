using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BG_Scroller : MonoBehaviour
{
    [Tooltip("배경 이미지 좌우 스크롤 속도 : 값이 클수록 빠름")]
    public float scrollSpeed;

    private RawImage rawImage; // Renderer 대신 RawImage 사용

    void Start()
    {
        // 이 오브젝트의 RawImage 컴포넌트를 가져옴
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        // RawImage의 uvRect를 직접 조작하여 스크롤 효과를 줌
        // x값만 시간의 흐름에 따라 변경하고, y, width, height는 그대로 유지
        float newX = rawImage.uvRect.x + scrollSpeed * Time.deltaTime;
        
        // uvRect는 0~1 사이의 값을 가지므로, 값이 계속 커지는 것을 방지하기 위해 나머지 연산 사용 (선택 사항이지만 권장)
        if (newX > 1)
        {
            newX -= 1;
        }

        rawImage.uvRect = new Rect(newX, 0f, 1f, 1f);
    }

}
