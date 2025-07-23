using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;    //파일 입출력을 위해 필요

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;   // 싱글톤 인스턴스

    [Header("플레이어 게임 데이터")]
    public PlayerData playerData;       // 현재 플레이어의 모든 데이터

    [Header("데이터베이스 참조")]
    public PetDatabase petDatabase;   // 펫 데이터베이스 연결
    private string savePath;            // 데이터 저장 경로

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
        LoadData(); // 게임 시작 시 데이터 로드
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
