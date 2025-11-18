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

    [Header("# 펫 컴포넌트 참조")]
    public PetSkillInstaller petSkillInstaller;

    [Header("# 캐릭터 관련 데이터베이스")]
    public List<CharaData> charaDatas;  // 모든 캐릭터 데이터를 담는 리스트

    [Header("# 보물상자 설정")]
    public int killsForNextChest = 100;    // 상자 스폰용 킬 카운트 간격
    private int currentKillThreshold;   // 다음 상자가 스폰될 목표 킬 카운트

    [Header("# 사운드 데이터")]
    [SerializeField] private SoundData gamePlaySoundData;

    public event Action<Enemy> OnBossSpawned;
    public event Action OnBossDefeated;

    private bool isPaused;  // 게임이 일시정지 상태인지 확인

    void Awake()
    // 초기화(선언)
    {
        instance = this;
        currentKillThreshold = killsForNextChest;
    }

    // PlayerDataManager로부터 선택 정보를 가져와 게임을 시작
    void Start()
    {
        if (AudioManager.instance != null)
        {
            // 1. (추가) 현재 재생 중인 BGM (메인 메뉴 BGM 등)을 먼저 멈춥니다.
            AudioManager.instance.StopBgm();

            // 2. (기존) 인게임 BGM 데이터를 로드하고 재생합니다.
            if (gamePlaySoundData != null)
            {
                AudioManager.instance.LoadAndPlaySceneSounds(gamePlaySoundData);
            }
        }

        int charId = PlayerDataManager.instance.playerData.selectedCharacterId;
        Debug.Log($"[GameManager] PlayerDataManager로부터 가져온 캐릭터 ID: {charId}");
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

        // 4. 선택된 펫 소환 및 스킬 설치 로직
        SpawnPetAndInstallSkill(petId);

        // 5. UI 및 게임 상태 설정
        //uiLevelUo.Select(playerId % 2);
        Resume();
        AudioManager.instance.PlaySfx("Select");
    }

    // 펫 소환과 스킬 설치 로직을 별도의 함수로 분리하여 관리합니다.
    void SpawnPetAndInstallSkill(int petId)
    {
        if (petId <= 0) // 펫 ID가 유효하지 않으면 (선택 안함 등)
        {
            Debug.Log("선택된 펫이 없어 펫 없이 게임을 시작합니다.");
            return;
        }

        PetData selectedPetData = PlayerDataManager.instance.petDatabase.GetPetByID(petId);

        if (selectedPetData == null)
        {
            Debug.LogError($"Pet ID {petId}에 해당하는 PetData를 PetDatabase에서 찾을 수 없습니다.");
            return;
        }

        if (selectedPetData.inGamePrefab == null) // PetData에 프리팹이 연결되었는지 확인
        {
            Debug.LogError($"Pet Id {petId}의 PetData에 'inGamePrefab'이 연결되지 않았습니다.");
            return;
        }

        // 1. 펫 프리팹을 씬에 생성
        GameObject petObject = Instantiate(selectedPetData.inGamePrefab, player.transform.position + new Vector3(1, 0, 0), Quaternion.identity);
        PetController spawnedPet = petObject.GetComponent<PetController>();

        if (spawnedPet == null)
        {
            Debug.LogError("소환된 펫 프리팹에 PetController 컴포넌트가 없습니다!");
            Destroy(petObject); // 잘못된 프리팹이므로 파괴
            return;
        }

        // 2. 소환된 펫 초기화 (플레이어를 알려줌)
        spawnedPet.Initialize(player.transform);
        Debug.Log($"[{selectedPetData.grade}] {selectedPetData.petName} (ID:{petId}) 펫을 소환했습니다.");

        // 3. PetSkillInstaller에게 스킬 설치 명령
        if (petSkillInstaller != null)
        {
            petSkillInstaller.InstallSkillForPet(spawnedPet);
        }
        else
        {
            Debug.LogWarning("GameManager에 PetSkillInstaller가 연결되지 않아 스킬을 설치할 수 없습니다.");
        }
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

        AudioManager.instance.StopBgm();
        AudioManager.instance.PlaySfx("Win");
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

        AudioManager.instance.StopBgm();
        AudioManager.instance.PlaySfx("Lose");
    }

    public void GameRetry()
    {
        // 씬을 로드하기 전에 시간을 1배속으로 되돌립니다.
        Time.timeScale = 1;

        // (선택사항) BGM이 멈추지 않는 문제를 위해 StopBgm을 명시적으로 호출합니다.
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBgm();
        }

        // Scene 0번 (아마도 MainMenuScene)을 로드합니다.
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

    public void AddKill()
    {
        if (!isLive) return;

        killCount++;

        if (killCount >= currentKillThreshold)
        {
            SpawnTreasureChest();
            currentKillThreshold += killsForNextChest;
        }
    }

    void SpawnTreasureChest()
    {
        GameObject chest = pool.Get(PoolManager.PoolCategory.Item, 5);

        Vector3 playerPos = player.transform.position;
        float spawnRadius = 5.0f;
         Vector3 randomDir = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
        chest.transform.position = playerPos + randomDir;

        Debug.Log($"<color=yellow><b>보물상자가 스폰되었습니다! 다음 상자는 {currentKillThreshold}킬 달성 시 나타납니다.</b></color>");
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

    // 지정된 위치에 데미지 팝업을 표시합니다.
    public void ShowDamagePopup(float damage, Vector3 position)
    {
        // PoolManager의 VFX 카테고리 0번에서 팝업을 가져옵니다.
        GameObject popupInstance = pool.Get(PoolManager.PoolCategory.VFX, 0);

        if (popupInstance != null)
        {
            // 팝업의 위치를 몬스터 위치로 설정 (약간의 랜덤 오프셋)
            popupInstance.transform.position = position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, 0);

            // 팝업 스크립트의 Setup 함수를 호출하여 데미지 값을 전달
            DamagePopup popupScript = popupInstance.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damage);
            }
        }
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
        AudioManager.instance.StopBgm();
        SceneManager.LoadScene("MainMenuScene");
    }
}
