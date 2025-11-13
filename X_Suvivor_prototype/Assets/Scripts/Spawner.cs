using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 인스펙터에서 시간대별 소환 정보를 설정하기 위한 보조 클래스들
[System.Serializable]
public class SpawnWave
{
    [Tooltip("이 웨이브가 시작되는 게임 시간 (초)")]
    public float startTime;
    [Tooltip("이 웨이브가 끝나는 게임 시간 (초)")]
    public float endTime;
    [Tooltip("소환할 몬스터의 데이터")]
    public MonsterData monsterData;
    [Tooltip("해당 몬스터의 소환 간격")]
    public float spawnInterval;

    // 내부적으로 사용할 타이머
    [HideInInspector]
    public float timer;
}

[System.Serializable]
public class SpawnEvent
{
    [Tooltip("몬스터가 소환될 정확한 게임 시간 (초)")]
    public float spawnTime;
    [Tooltip("소환할 몬스터의 데이터")]
    public MonsterData monsterData;

    // 소환 완료 여부 체크
    [HideInInspector]
    public bool isSpawned;

    // 경고 연출이 시작되었는지 확인하는 플래그
    [HideInInspector]
    public bool isWarningTriggerd;
}

public class Spawner : MonoBehaviour
{
    public PoolManager poolManager;

    [Header("지속 소환 웨이브 설정")]
    public List<SpawnWave> spawnWaves; // 시간에 따라 지속적으로 소환될 몬스터 목록

    [Header("이벤트 소환 설정 (엘리트, 보스)")]
    public List<SpawnEvent> spawnEvents; // 특정 시간에 한 번만 소환될 몬스터 목록

    [Header("보스 연출 설정")]
    [Tooltip("씬에 있는 Post-Process Volume 오브젝트를 연결해주세요.")]
    public Volume postProcessVolume;

    private Vignette vignette; // 비네트 효과를 저장할 변수
    private Color originalVignetteColor; // 원래 비네트 색상을 저장할 변수
    private float originalVignetteIntensity; // 원래 비네트 강도를 저장할 변수

    void Start()
    {
        // 게임 시작 시 Post-Process Volume에서 Vignette 컴포넌트를 찾기
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out vignette))
        {
            // 나중에 원래대로 되돌리기 위한 초기값 저장
            originalVignetteColor = vignette.color.value;
            originalVignetteIntensity = vignette.intensity.value;
        }
        else
        {
            Debug.LogError("보스 연출을 위한 Post-Process Volume 또는 Vignette를 찾을 수 없습니다.");
        }

        foreach (var evt in spawnEvents)
        {
            evt.isSpawned = false;
            evt.isWarningTriggerd = false;
        }
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        float gameTime = GameManager.instance.gameTime;

        // 1. 지속 소환 웨이브 처리
        foreach (var wave in spawnWaves)
        {
            // 현재 게임 시간이 웨이브의 시작과 끝 시간 사이에 있는지 확인
            if (gameTime >= wave.startTime && gameTime < wave.endTime)
            {
                wave.timer += Time.deltaTime;
                if (wave.timer >= wave.spawnInterval)
                {
                    wave.timer = 0;
                    Spawn(wave.monsterData);
                }
            }
        }

        // 2. 이벤트 소환 처리
        foreach (var evt in spawnEvents)
        {
            // 보스 몬스터이고, 아직 경고가 시작되지 않았으며, 출현 5초 전이라면
            if (evt.monsterData.tier == MonsterData.MonsterTier.Boss && !evt.isWarningTriggerd && gameTime >= evt.spawnTime - 5f)
            {
                evt.isWarningTriggerd = true;
                StartCoroutine(BossVignetteEffect(evt.spawnTime));
            }
            
            // 아직 소환되지 않았고, 소환 시간이 되었다면
            if (!evt.isSpawned && gameTime >= evt.spawnTime)
            {
                evt.isSpawned = true; // 한 번만 소환되도록 플래그 설정
                Spawn(evt.monsterData);
            }
        }
    }

    void Spawn(MonsterData monsterData)
    {
        // MonsterData에 저장된 Tier 정보를 바탕으로 올바른 PoolCategory를 선택
        PoolManager.PoolCategory category;
        switch (monsterData.tier)
        {
            case MonsterData.MonsterTier.Elite:
                category = PoolManager.PoolCategory.EliteMonster;
                break;
            case MonsterData.MonsterTier.Boss:
                category = PoolManager.PoolCategory.BossMonster;
                break;
            default: // MonsterTier.Normal
                category = PoolManager.PoolCategory.NormalMonster;
                break;
        }

        // PoolManager에 몬스터 카테고리와 데이터에 저장된 인덱스를 넘겨줌
        GameObject monster = poolManager.Get(category, monsterData.PoolManagerIndex);
        monster.transform.position = GetRandomSpawnPosition();

        // Enemy 스크립트에 MonsterData를 전달하여 초기화
        Enemy enemy = monster.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Init(monsterData);
        }

        if (monsterData.tier == MonsterData.MonsterTier.Boss)
        {
            Debug.Log($"<color=red><b>[Spawner] 보스 생성 확인! 이름: {monsterData.Name}, 등급: {monsterData.tier}. GameManager에 알림을 보냅니다.</b></color>");
            GameManager.instance.NotifyBossSpawned(enemy);
        }
    }

    IEnumerator BossVignetteEffect(float bossSpawnTime)
    {
        if (vignette == null) yield break;  // 비네트를 찾지 못했다면 코루틴 종료

        Color warningColor = new Color(1f, 0.5f, 0f);
        float warningBlinkSpeed = 5f;

        vignette.color.Override(warningColor);

        // gameTime을 기준으로 보스가 출현할 때까지 반복
        while (GameManager.instance.gameTime < bossSpawnTime)
        {
            // Sin 함수를 이용해 0과 1 사이를 부드럽게 왕복하는 값 생성
            float blink = (Mathf.Sin(Time.time * warningBlinkSpeed) + 1) / 2f;
            // 원래 강도에 0 ~ 0.2 사이의 값을 더해 깜빡이는 효과 주기
            vignette.intensity.Override(originalVignetteIntensity + blink * 0.2f);
            yield return null;  //다음 프레임까지 대기
        }

        // 보스 출현 후 3초 : 붉은색 점멸
        Color spawnColor = Color.red;
        float spawnBlinkSpeed = 15f;

        vignette.color.Override(spawnColor);

        float effectEndTime = GameManager.instance.gameTime + 3f;
        while (GameManager.instance.gameTime < effectEndTime)
        {
            float blink = (Mathf.Sin(Time.time * spawnBlinkSpeed) + 1) / 2f;
            vignette.intensity.Override(originalVignetteIntensity + blink * 0.3f);
            yield return null;
        }

        // 연출 종료 후 : 원래 상태로 복구
        Debug.Log("보스 비네트 연출 종료. 원래 상태로 복구함.");
        vignette.color.Override(originalVignetteColor);
        vignette.intensity.Override(originalVignetteIntensity);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPos = GameManager.instance.player.transform.position;
        float spawnRadius = 15.0f;

        Vector3 randomDir = Random.insideUnitCircle.normalized * spawnRadius;
        return playerPos + randomDir;
    }
}