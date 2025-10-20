using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;    //파일 입출력을 위해 필요

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;   // 싱글톤 인스턴스

    [Header("플레이어 게임 데이터")]
    public PlayerData playerData;       // 현재 플레이어의 모든 데이터

    [Header("데이터베이스 참조")]
    public PetDatabase petDatabase;   // 펫 데이터베이스 연결

    [Header("메인 메뉴 UI 참조 (에디터에서 연결)")]
    [Tooltip("메인 메뉴 씬에 있는 환영 팝업 UI 오브젝트")]
    public GameObject welcomePopup; // 환영 팝업 UI를 연결할 변수

    private string savePath;           // 데이터 저장 경로

    public event Action OnPlayerDataUpdated;    // 재화 변경 이벤트 선언(인스턴스 기반)

    void Awake()
    {
        // ===== 싱글톤 패턴 =====
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 씬이 바뀌어도 이 오브젝트는 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);        // 이미 인스턴스가 존재한다면 새로 생긴 것은 폐기
            return;
        }
        // =======================

        savePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
        Debug.Log("데이터 저장 경로: " + savePath);
        LoadData(); // 게임 시작 시 데이터 로드
    }

    // Awake 이후, 다른 스크립트들이 준비된 후 실행됩니다.
    void Start()
    {
        // 팝업을 띄울지 여기서 결정합니다.
        CheckForWelcomeGift();
    }

    private void CheckForWelcomeGift()
    {
        // 저장된 데이터가 있고, 보유한 펫이 하나도 없다면 신규 유저로 판단합니다.
        if (playerData != null && (playerData.ownedPetIDs == null || playerData.ownedPetIDs.Count == 0))
        {
            Debug.Log("보유한 펫이 없어 신규 유저로 판단, 환영 팝업을 띄웁니다.");

            // welcomePopup이 Inspector에 연결되어 있다면 활성화시킵니다.
            if (welcomePopup != null)
            {
                welcomePopup.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Welcome Popup이 PlayerDataManager에 연결되지 않았습니다.");
            }
        }
    }

    public void UnlockPet(int petId)
    {
        // ownedPetIds 리스트가 null이면 새로 생성해줍니다. (안전장치)
        if (playerData.ownedPetIDs == null)
        {
            playerData.ownedPetIDs = new List<int>();
        }

        // 이미 보유한 펫이 아니라면 목록에 추가합니다.
        if (!playerData.ownedPetIDs.Contains(petId))
        {
            playerData.ownedPetIDs.Add(petId);
            Debug.Log($"펫 ID: {petId} 해금 및 보유 목록에 추가 완료!");
        }
    }

    public void AddGold(long amount)
    {
        playerData.gold += amount;
        NotifyDataUpdate(); // 데이터 변경 알림
        Debug.Log($"{amount}만큼의 돈이 추가됩니다.");
        SaveData();
    }

    public void AddGems(long amount)
    {
        playerData.gems += amount;
        NotifyDataUpdate(); // 데이터 변경 알림
        Debug.Log($"{amount}만큼의 젬이 추가됩니다.");
        SaveData();
    }

    public bool SpendGems(long amount)
    {
        if (playerData.gems >= amount)
        {
            playerData.gems -= amount;
            NotifyDataUpdate(); // 데이터 변경 알림
            Debug.Log($"가챠 실행 성공.");
            SaveData();
            return true;    // 구매 성공
        }
        Debug.LogError($"가챠 실행 실패.");
        return false; // 구매 실패
    }

    // 외부에 데이터 변경을 알리는 함수
    private void NotifyDataUpdate()
    {
        if (OnPlayerDataUpdated != null)
        {
            OnPlayerDataUpdated();
        }
        Debug.Log("플레이어 데이터 변경 알림을 보냈습니다.");
    }

    // ===== 데이터 저장 및 로드 =====
    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            // 저장된 파일이 있을 시, 파일의 내용을 읽어와서 PlayerData 객체로 변환(역직렬화)
            string json = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            // 저장된 파일이 없으면(최초 실행 시), 새로운 PlayerData 생성
            Debug.Log("저장된 데이터가 없어 새로 생성합니다.");
            playerData = new PlayerData();
            SaveData(); // 처음 생성 시 바로 한 번 저장
        }
        NotifyDataUpdate();
    }

    public void SaveData()
    {
        // PlayerData 객체를 JSON 문자열로 변환(직렬화)
        string json = JsonUtility.ToJson(playerData, true);
        // 변환된 JSON 문자열을 파일에 저장
        File.WriteAllText(savePath, json);
        Debug.Log("플레이어 데이터를 파일에 저장했습니다.");
    }

    // 게임이 종료될 때 자동으로 저장되도록 함
    void OnApplicationQuit()
    {
        SaveData();
    }
}
