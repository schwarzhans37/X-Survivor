using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultUI : MonoBehaviour
{
    public GameObject resultPanel;      // 결과창 전체 패널
    public Transform cardContainer;     // 결과 카드들이 생성될 위치
    public GameObject resultCardPrefab; // 결과 카드 1개의 프리팹
    public Button closeButton;

    // GachaManager가 이 함수를 호출하여 결과창을 띄움
    public void ShowResults(List<GachaResult> results)
    {
        resultPanel.SetActive(true);
        StartCoroutine(ShowResultsRoutine(results));
    }

    private IEnumerator ShowResultsRoutine(List<GachaResult> results)
    {
        // 기존 카드들 삭제
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 10회 뽑기일 경우 "Skip" 버튼 활성화
        // ...

        // 각 결과를 하나씩 애니메이션과 함께 표시
        foreach (var result in results)
        {
            GameObject cardGO = Instantiate(resultCardPrefab, cardContainer);
            // GachaResultCard 스크립트에 데이터 전달
            //cardGO.GetComponent<GachaResultCard>().Setup(result); 
            
            // 카드 하나가 나타나는 연출 (예: 0.2초 대기)
            yield return new WaitForSeconds(0.2f);
        }
        
        // 모든 카드가 나온 후 닫기 버튼 활성화
        closeButton.gameObject.SetActive(true);
    }
    
    public void CloseResultPanel()
    {
        resultPanel.SetActive(false);
    }
}
