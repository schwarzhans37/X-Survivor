using UnityEngine;
using UnityEngine.UI; // Image를 사용하기 위해 필요
using System;

public class PetWalker : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("걷기 애니메이션 프레임 스프라이트 배열")]
    public Sprite[] walkFrames;
    [Tooltip("애니메이션 재생 속도 (초당 프레임 수)")]
    public float animationFPS = 10f;

    // Image 컴포넌트 참조
    private Image petImage;

    // 이동 관련 변수 (기존과 동일)
    private float moveSpeed;
    private float midPointX;
    private float endPointX;
    private Action<GameObject> onReachedMid;
    private Action<GameObject> onReachedEnd;
    private bool hasReachedMid = false;

    // 애니메이션 재생 관련 변수
    private int currentFrame = 0;
    private float frameTimer = 0f;

    void Awake()
    {
        // 자식 또는 자신에게서 Image 컴포넌트를 찾습니다.
        petImage = GetComponentInChildren<Image>();
        if (petImage == null)
        {
            petImage = GetComponent<Image>(); // 혹시 Image가 부모에 있을 경우
        }

        if (petImage == null)
        {
            Debug.LogError("PetWalker가 제어할 Image 컴포넌트를 찾을 수 없습니다!");
            enabled = false; // 스크립트 비활성화
        }
    }

    public void Initialize(float speed, float midX, float endX, Action<GameObject> midCallback, Action<GameObject> endCallback)
    {
        moveSpeed = speed;
        midPointX = midX;
        endPointX = endX;
        onReachedMid = midCallback;
        onReachedEnd = endCallback;
        hasReachedMid = false;

        // 애니메이션 초기화
        currentFrame = 0;
        frameTimer = 0f;
        UpdateSprite(); // 첫 프레임 설정
    }

    void Update()
    {
        // 이동 로직 (기존과 동일)
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        if (!hasReachedMid && transform.position.x >= midPointX)
        {
            hasReachedMid = true;
            onReachedMid?.Invoke(gameObject);
        }
        if (transform.position.x >= endPointX)
        {
            onReachedEnd?.Invoke(gameObject);
        }

        // 애니메이션 로직
        AnimateWalk();
    }

    // 걷기 애니메이션 처리 함수
    void AnimateWalk()
    {
        // 프레임 배열이 없거나 Image 컴포넌트가 없으면 실행 안 함
        if (walkFrames == null || walkFrames.Length == 0 || petImage == null) return;

        // 타이머 업데이트
        frameTimer += Time.deltaTime;

        // 다음 프레임으로 넘어갈 시간이 되었는지 확인 (1 / FPS = 프레임당 시간)
        if (frameTimer >= (1f / animationFPS))
        {
            frameTimer -= (1f / animationFPS); // 타이머 리셋 (남은 시간 고려)
            currentFrame = (currentFrame + 1) % walkFrames.Length; // 다음 프레임 인덱스 (배열 끝에 도달하면 처음으로)
            UpdateSprite(); // 스프라이트 업데이트
        }
    }

    // 현재 프레임에 맞는 스프라이트를 Image에 적용하는 함수
    void UpdateSprite()
    {
        if (petImage != null && walkFrames != null && currentFrame < walkFrames.Length)
        {
            petImage.sprite = walkFrames[currentFrame];
        }
    }
}


