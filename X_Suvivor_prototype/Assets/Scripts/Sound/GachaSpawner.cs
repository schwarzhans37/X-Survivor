using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaSpawner : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    public GameObject cardPrefab;      // GachaResultCard_Prefab
    public Transform cardContainer;    // CardContainer

    [Header("Timing")]
    public float preDelay = 0.15f;     // 시작 누르고 살짝 텀
    public float revealInterval = 0.18f; // 카드 간 텀

    Coroutine running;

    // 버튼에서 연결
    public void OnClickDraw1() { if (running == null) running = StartCoroutine(SpawnRoutine(1)); }
    public void OnClickDraw10() { if (running == null) running = StartCoroutine(SpawnRoutine(10)); }

    IEnumerator SpawnRoutine(int count)
    {
        // 기존 카드 정리(있으면)
        foreach (Transform t in cardContainer) Destroy(t.gameObject);

        // 시작 효과음
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Gacha_Start);
        yield return new WaitForSeconds(preDelay);

        // 너의 결과 로직으로 바꿔
        var results = Roll(count);

        // 순차 생성 + 결과음
        for (int i = 0; i < results.Count; i++)
        {
            var go = Instantiate(cardPrefab, cardContainer);
            SetupCard(go, results[i]);           // 스프라이트/색 세팅(네 함수)

            // ★ 이 타이밍에 결과음
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Gacha_Result);

            if (i < results.Count - 1)
                yield return new WaitForSeconds(revealInterval);
        }

        running = null;
    }

    // ===== 샘플(네 프로젝트에 맞게 교체) =====
    List<int> Roll(int n) { var l = new List<int>(n); for (int i = 0; i < n; i++) l.Add(Random.Range(0, 1000)); return l; }
    void SetupCard(GameObject go, int resultId)
    {
        // 아이콘/등급 세팅. 이미지를 여러 개 가진 프리팹이면 여기서 활성화
        var img = go.GetComponentInChildren<Image>();
        if (img) { img.sprite = /* GetSprite(resultId) */ null; img.SetNativeSize(); }
    }
}
