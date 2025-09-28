using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public class Spawner : MonoBehaviour
{
    public PoolManager poolManager;

    [Header("지속 소환 웨이브 설정")]
    public List<SpawnWave> spawnWaves; // 시간에 따라 지속적으로 소환될 몬스터 목록

    [Header("이벤트 소환 설정 (엘리트, 보스)")]
    public List<SpawnEvent> spawnEvents; // 특정 시간에 한 번만 소환될 몬스터 목록

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

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPos = GameManager.instance.player.transform.position;
        float spawnRadius = 15.0f;

        Vector3 randomDir = Random.insideUnitCircle.normalized * spawnRadius;
        return playerPos + randomDir;
    }
}