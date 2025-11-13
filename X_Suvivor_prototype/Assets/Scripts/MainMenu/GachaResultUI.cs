using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/*
    가챠 결과 목록을 받아, 카드들을 순서대로 보여주는 UI 관리 스크립트
*/
public class GachaResultUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject resultPanel;      // 결과창 전체 패널
    public Transform cardContainer_DrawOne;     // 1회 뽑기용 컨테이너
    public Transform cardContainer_DrawTen;     // 10회 뽑기용 컨테이너
    public Button closeButton;

    [Header("Timing")]
    public float preDelay = 0.15f;      // 시작음 후 살짝 텀
    public float revealInterval = 0.18f; // 카드 간 텀

    [Header("SFX")]
    public bool playSfx = true;
    public string startSfxName = "GachaStart";
    public string revealSfxName = "GachaResult";
    public string closeSfxName = "ButtonClick";

    // GachaManager가 이 함수를 호출하여 결과창을 띄움
    public void ShowResults(List<GachaResult> results)
    {
        resultPanel.SetActive(true);
        closeButton.gameObject.SetActive(false);
        StartCoroutine(ShowResultsRoutine(results));
    }

    private IEnumerator ShowResultsRoutine(List<GachaResult> results)
    {
        // 두 컨테이너 모두 정리 및 비활성화하여 초기화
        foreach (Transform child in cardContainer_DrawOne) Destroy(child.gameObject);
        foreach (Transform child in cardContainer_DrawTen) Destroy(child.gameObject);
        cardContainer_DrawOne.gameObject.SetActive(false);
        cardContainer_DrawTen.gameObject.SetActive(false);

        // 가챠 갯수에 따라 사용할 컨테이너 결정 및 활성화
        Transform targetContainer;
        if (results.Count == 1)
        {
            targetContainer = cardContainer_DrawOne;
            targetContainer.gameObject.SetActive(true);
        }
        else
        {
            targetContainer = cardContainer_DrawTen;
            targetContainer.gameObject.SetActive(true);
        }

        // 시작 효과음 1회
        if (playSfx && AudioManager.instance != null)
            AudioManager.instance.PlaySfx(startSfxName);

        // 살짝 텀
        if (preDelay > 0f) yield return new WaitForSeconds(preDelay);

        // 각 결과를 하나씩 표시
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var petData = result.petData;

            // PetDAta에 스플래시 아트 프리팹이 있는지 확인
            if (petData != null && petData.splashArtPrefab != null)
            {
                // splashArtPrefab을 CardContainer의 자식으로 생성
                Instantiate(petData.splashArtPrefab, targetContainer);
            }
            else
            {
                Debug.LogWarning($"표시할 스플래시 아트가 없습니다. PetID : {(petData != null ? petData.id.ToString() : "N/A")}");
            }

            // 카드 등장 타이밍에 결과음
            if (playSfx && AudioManager.instance != null)
                AudioManager.instance.PlaySfx(revealSfxName);

            // 다음 카드까지 텀
            if (i < results.Count - 1 && revealInterval > 0f)
                yield return new WaitForSeconds(revealInterval);
        }

        // 전부 나온 뒤 닫기 버튼 활성화
        closeButton.gameObject.SetActive(true);
    }
    
    public void CloseResultPanel()
    {
        if (playSfx && AudioManager.instance != null)
            AudioManager.instance.PlaySfx(closeSfxName); // 선택

        resultPanel.SetActive(false);
    }
}
