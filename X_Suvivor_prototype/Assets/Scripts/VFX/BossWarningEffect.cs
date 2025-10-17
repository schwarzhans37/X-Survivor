using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BossWarningEffect : MonoBehaviour
{
    public Volume postProcessVolume;
    private Vignette vignette;

    void Start()
    {
        // Volume 프로파일에서 Vignette 컴포넌트(Override)를 찾아 저장
        if (postProcessVolume != null)
        {
            // TryGet<>을 사용
            postProcessVolume.profile.TryGet(out vignette);
        }

        if (vignette == null)
        {
            Debug.LogError("Vignette Override를 찾을 수 없습니다! Global Volume의 Profile에 Vignette가 추가되었는지, 속성들이 체크(Override)되었는지 확인하세요.");
        }
        else
        {
            // 시작할 때는 비활성화 상태.
            vignette.active = false;
        }
    }

    // 외부(BossHPUI)에서 이 함수를 호출하여 효과를 시작.
    public void StartWarningEffect()
    {
        if (vignette != null)
        {
            StopAllCoroutines(); // 혹시 이전에 실행중인 코루틴이 있다면 중지
            StartCoroutine(PulsingVignetteCoroutine());
        }
    }

    private IEnumerator PulsingVignetteCoroutine()
    {
        // 1. 효과 활성화
        vignette.active = true;

        float duration = 4.0f;     // 총 효과 지속 시간 (4초)
        int pulses = 4;            // 깜빡이는 횟수
        float maxIntensity = 0.55f; // 비네트 최대 강도 (0~1 사이)

        float pulseDuration = duration / (pulses * 2); // 한 번 밝아지거나 어두워지는 데 걸리는 시간

        // 2. 지정된 횟수만큼 깜빡이기
        for (int i = 0; i < pulses; i++)
        {
            // 밝아지기 (Intensity 0 -> maxIntensity)
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                // VolumeParameter는 .value로 실제 값에 접근.
                vignette.intensity.value = Mathf.Lerp(0, maxIntensity, elapsed / pulseDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            vignette.intensity.value = maxIntensity;

            // 어두워지기 (Intensity maxIntensity -> 0)
            elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                vignette.intensity.value = Mathf.Lerp(maxIntensity, 0, elapsed / pulseDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            vignette.intensity.value = 0;
        }

        // 3. 효과가 끝나면 비활성화.
        vignette.active = false;
    }
}
