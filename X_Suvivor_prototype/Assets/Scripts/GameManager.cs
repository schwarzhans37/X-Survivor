using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# 게임 컨트롤 설정")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("# 플레이어 정보")]
    public int playerId;
    public int level;
    public int killCount;
    public int nowExp;
    public int[] needExp;
    public long acquiredGold;   // 게임 중 획득한 골드
    public long acquiredGems;   // 게임 중 획득한 젬

    [Header("# 게임 오브젝트들")]
    public Player player;
    public PoolManager pool;
    public LevelUp uiLevelUo;
    public Result uiResult;
    public GameObject enemyCleaner;
    public GameObject pauseMenuPanel;

    [Header("# 캐릭터 관련 데이터베이스")]
    public List<CharaData> charaDatas;  // 모든 캐릭터 데이터를 담는 리스트

    public event Action<Enemy> OnBossSpawned;
    public event Action OnBossDefeated;

    private bool isPaused;  // 게임이 일시정지 상태인지 확인

    void Awake()
    // 초기화(선언)
    {
        instance = this;
    }

    // PlayerDataManager로부터 선택 정보를 가져와 게임을 시작
    void Start()
    {
        int charId = PlayerDataManager.instance.playerData.selectedCharacterId;
        Debug.Log($"[GameManager] PlayerDataManager로부터 가져온 캐릭터 ID: {charId}"); // <--- 확인용 로그 추가
        int petId = PlayerDataManager.instance.playerData.selectedPetId;

        GameStart(charId, petId);
    }

    public void GameStart(int charId, int petId)
    {
        playerId = charId;
        Debug.Log($"[GameManager.GameStart] ID: {charId}로 게임 시작");

        // 1. ID에 맞는 캐릭터 데이터 찾기
        CharaData charaData = charaDatas.Find(data => data.charaId == charId);

        if (charaData != null)
        {
            // 2. 플레이어 오브젝트 활성화 & 초기화
            player.gameObject.SetActive(true);
            player.Init(charId);

            // 3. 찾은 데이터의 시작 무기를 플레이어에게 장착시킴
            if (charaData.startingWeapon != null)
            {
                player.EquipWeapon(charaData.startingWeapon);
            }
        }
        else
        {
            Debug.LogError($"ID {charId}에 해당하는 CharaData를 찾을 수 없습니다.");
        }

        // 4. 펫 설정(임시)
        if (petId != -1)    // 선택된 펫이 있다면....
        {
            // 펫을 생성하고 설정하는 코드 추가 필요
            /* 예시 :
                PetData data = PetDatabase.instance.GetByID(petId);
                GameObject petObject = pool.Get(펫 프리팹 인덱스);
                petObject.GetComponent<Pet>().Init(data, player.transform);
            */
            Debug.Log($"펫 Id: {petId}와 함께 게임을 시작합니다.");
        }

        // 5. UI 및 게임 상태 설정
        //uiLevelUo.Select(playerId % 2);
        Resume();
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        PlayerDataManager.instance.AddGold(acquiredGold);
        PlayerDataManager.instance.AddGems(acquiredGems);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        PlayerDataManager.instance.AddGold(acquiredGold);
        PlayerDataManager.instance.AddGems(acquiredGems);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        // 일시정지 토글 로직
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            Debug.Log("게임이 일시정지 되었습니다.");
        }

        if (!isLive || isPaused)    // 일시정지 상태 혹은 사망 시 게임 시간 정지
        {
            return;
        }
        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    public void NotifyBossSpawned(Enemy boss)
    {
        OnBossSpawned?.Invoke(boss);    // 보스가 등장했음을 알림
    }

    public void NotifyBossDefeated()
    {
        OnBossDefeated?.Invoke();       // 보스가 사망했음을 알림
    }

    public void GetExp(int amount)
    {
        if (!isLive)
            return;

        nowExp += amount;

        while (nowExp >= needExp[Mathf.Min(level, needExp.Length - 1)])
        {
            // 1. 현재 레벨에서 필요했던 경험치 만큼만 차감
            nowExp -= needExp[Mathf.Min(level, needExp.Length - 1)];
            // 2. 레벨업
            level++;
            // 3. 레벨업 UI 표시
            uiLevelUo.show();
        }
    }

    public void GetGold(int amount)
    {
        if (!isLive)
            return;
        acquiredGold += amount;
        // TODO: HUD에 골드 획득량 표시 UI 업데이트
    }

    public void GetGems(int amount)
    {
        if (!isLive)
            return;
        acquiredGems += amount;
        // TODO: HUD에 젬 획득량 표시 UI 업데이트
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;     //유니티의 시간 흐름 배율
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        pauseMenuPanel.SetActive(true);
        Debug.Log("일시정지 메뉴 활성화.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        pauseMenuPanel.SetActive(false);
        Debug.Log("일시정지 메뉴 비활성화.");
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenuScene");
    }
}
