using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    가챠 결과 목록을 받아, 카드들을 순서대로 보여주는 UI 관리 스크립트
*/
public class GachaResultUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject resultPanel;      // 결과창 전체 패널
    public Transform cardContainer;     // 카드들이 들어갈 부모
    public GameObject resultCardPrefab; // 결과 카드 1개 프리팹
    public Button closeButton;

    [Header("Timing")]
    public float preDelay = 0.15f;      // 시작음 후 살짝 텀
    public float revealInterval = 0.18f; // 카드 간 텀

    [Header("SFX")]
    public bool playSfx = true;
    public AudioManager.Sfx startSfx = AudioManager.Sfx.Gacha_Start;
    public AudioManager.Sfx revealSfx = AudioManager.Sfx.Gacha_Result;
    public AudioManager.Sfx closeSfx = AudioManager.Sfx.UI_Click; // 선택

    // GachaManager가 이 함수를 호출하여 결과창을 띄움
    public void ShowResults(List<GachaResult> results)
    {
        resultPanel.SetActive(true);
        closeButton.gameObject.SetActive(false);
        StartCoroutine(ShowResultsRoutine(results));
    }

    private IEnumerator ShowResultsRoutine(List<GachaResult> results)
    {
        // 기존 카드들 정리
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        // 시작 효과음 1회
        if (playSfx && AudioManager.instance != null)
            AudioManager.instance.PlaySfx(startSfx);

        // 살짝 텀
        if (preDelay > 0f) yield return new WaitForSeconds(preDelay);

        // 각 결과를 하나씩 표시
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];

            // 카드 생성 + 데이터 세팅
            GameObject cardGO = Instantiate(resultCardPrefab, cardContainer);
            var card = cardGO.GetComponent<GachaResultCard>();
            if (card != null) card.Setup(result);

            // 카드 등장 타이밍에 결과음
            if (playSfx && AudioManager.instance != null)
                AudioManager.instance.PlaySfx(revealSfx);

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
            AudioManager.instance.PlaySfx(closeSfx); // 선택

        resultPanel.SetActive(false);
    }
}
