using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GachaRateData gachaRateData;     // 인스펙터에서 연결할 확률 테이블
    public GachaResultUI gachaResultUI;     // 결과 표시를 위한 UI 참조
    public MessageBoxPanel notEnoughGemsMessage;    // 재화 부족 메세지 박스 참조

    // 1회 뽑기
    public void DrawOne()
    {
        Debug.Log("1회 뽑기버튼이 클릭됨.");
        if (!PlayerDataManager.instance.SpendGems(100L))
        {
            Debug.LogError("1회 뽑기에 필요한 Gems가 부족함.");
            notEnoughGemsMessage.Show();
            return;
        }
        Debug.Log("가챠 1회 실행 성공");

        List<GachaResult> results = new List<GachaResult>();
        results.Add(PerformDraw());    // 1회 뽑기 실행

        gachaResultUI.ShowResults(results);     // 결과 UI에 표시
    }

    public void DrawTen()
    {
        Debug.Log("10회 뽑기버튼이 클릭됨.");
        if (!PlayerDataManager.instance.SpendGems(1000L))
        {
            Debug.LogError("10회 뽑기에 필요한 Gems가 부족함.");
            notEnoughGemsMessage.Show();
            return;
        }
        Debug.Log("가챠 10회 실행 성공");

        List<GachaResult> results = new List<GachaResult>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(PerformDraw()); // 10회 뽑기 실행
        }

        gachaResultUI.ShowResults(results);    // 결과 UI에 표시
    }

    private GachaResult PerformDraw()
    {
        // 1. 등급 결정
        float randomPoint = Random.Range(0, 100f);
        float cumulative = 0f;

        GachaRateData.GachaRate selectedGrade = null;

        foreach (var rate in gachaRateData.rates)
        {
            cumulative += rate.probability;
            if (randomPoint <= cumulative)
            {
                selectedGrade = rate;
                break;
            }
        }

        // 2. 결정된 등급 내에서 펫 1마리 랜덤 선택
        int randomIndex = Random.Range(0, selectedGrade.petIds.Count);
        int selectedPetId = selectedGrade.petIds[randomIndex];

        // 3. 펫 데이터 가져오기
        var petDB = PlayerDataManager.instance.petDatabase;
        PetData drawPet = petDB.GetPetByID(selectedPetId);

        // 4. 플레이어 데이터 업데이트 및 결과 생성
        bool isNew = !PlayerDataManager.instance.playerData.ownedPetIDs.Contains(selectedPetId);
        if (isNew)
        {
            // 처음 얻은 펫이라면 보유 목록에 추가
            PlayerDataManager.instance.playerData.ownedPetIDs.Add(selectedPetId);
        }
        else
        {
            // 중복 펫은 카운트 증가 또는 재화로 변환(차후 구현)
            Debug.Log($"중복 펫 획득: {drawPet.petName} (ID: {drawPet.id})");
        }

        PlayerDataManager.instance.SaveData();  //  데이터 변경 후 저장

        return new GachaResult { petData = drawPet, isNew = isNew };
    }
}
