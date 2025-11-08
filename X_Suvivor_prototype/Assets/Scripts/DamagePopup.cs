using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    [Tooltip("데미지 숫자를 표시할 Text 컴포넌트")]
    public TMPro.TextMeshProUGUI damageText;

    [Header("팝업 설정")]
    public float moveSpeed = 2f;    // 위로 떠오르는 속도
    public float fadeOutTime = 1f;  // 사라지는 데 걸리는 시간
    public float moveYAmount = 0.8f;  // 총 이동할 Y 거리

    private Color startColor;
    private float timer = 0f;
    private Vector3 startPosition;

    void Awake()
    {
        if (damageText != null)
        {
            startColor = damageText.color;
        }
    }

    // GameManager가 이 함수를 호출하여 팝업을 설정합니다.
    public void Setup(float damageAmount)
    {
        if (damageText != null)
        {
            damageText.text = Mathf.FloorToInt(damageAmount).ToString(); // 데미지를 정수로 표시
            damageText.color = startColor; // 색상 초기화
        }

        startPosition = transform.position;
        timer = 0f;

        // 팝업이 생성될 때 바로 코루틴을 시작합니다.
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        float fadeTimer = 0f;

        while (fadeTimer < fadeOutTime)
        {
            // 1. 위로 이동
            transform.position = Vector3.Lerp(startPosition, startPosition + Vector3.up * moveYAmount, fadeTimer / fadeOutTime);

            // 2. 서서히 투명하게 (Fade Out)
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);
            damageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            fadeTimer += Time.deltaTime;
            yield return null;
        }

        // 3. 1초가 지나면 팝업을 비활성화 (오브젝트 풀로 반환)
        gameObject.SetActive(false);
    }
}