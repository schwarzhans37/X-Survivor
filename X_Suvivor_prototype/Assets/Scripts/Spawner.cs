using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public MonsterDatabase monsterDatabase;
    public RuntimeAnimatorController[] animCon;

    public float spawnInterval = 3f;
    private float timer;
    private bool isReady = false;   // 스폰 준비 완료 플래그

    [System.Serializable]
    public class EliteSchedule
    {
        public int spawnTime;        // 초 단위
        public int monsterId;        // 예: 1000
        public bool spawned = false; // 중복 스폰 방지
    }

    public List<EliteSchedule> eliteSchedule = new List<EliteSchedule>();

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();

        if (monsterDatabase == null || monsterDatabase.monsters.Count == 0)
        {
            Debug.LogError("[Spawner] MonsterDatabase가 초기화되지 않았습니다.");
        }
    }

    void Start()
    {
        if (monsterDatabase == null)
        {
            Debug.LogError("[Spawner] MonsterDatabase가 연결되지 않았습니다.");
            return;
        }

        // 데이터 로딩이 끝날 때까지 잠깐 대기 (지연 스폰 시작)
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);  // 데이터 로딩 기다리기

        isReady = true;     // 스폰을 시작할 준비가 되었음을 알림
        timer = spawnInterval;  // 바로 스폰하도록 타이머 초기화
    }

    void Update()
    {
        // isReady 플래그나 GameManager의 isLive를 통해 스폰 여부 제어
        if (!isReady || !GameManager.instance.isLive)
            return;
        
        if (monsterDatabase == null || monsterDatabase.monsters.Count == 0)
            return;

        float currentTime = GameManager.instance.gameTime;
        timer += Time.deltaTime;

        // 일반 몬스터 스폰
        if (timer > spawnInterval)
        {
            timer = 0;
            SpawnNormal(currentTime);
        }

        // 엘리트 몬스터 스폰
        foreach (var schedule in eliteSchedule)
        {
            if (!schedule.spawned && currentTime >= schedule.spawnTime)
            {
                schedule.spawned = true;
                SpawnElite(schedule.monsterId);
            }
        }
    }

    void SpawnNormal(float currentTime)
    {
        int monsterId = GetNormalMonsterIdByTime(currentTime);
        MonsterData data = monsterDatabase.GetByID(monsterId);

        if (data == null)
        {
            Debug.LogWarning($"[일반 몬스터] ID {monsterId}�에 해당하는 데이터가 없습니다.");
            return;
        }

        GameObject enemy = GameManager.instance.pool.Get(1); // 일반 몬스터는 풀 인덱스 1
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        int animIndex = monsterDatabase.monsters.FindIndex(m => m.Id == data.Id);
        if (animIndex >= 0 && animIndex < animCon.Length)
        {
            data.animator = animCon[animIndex];
        }

        enemy.GetComponent<Enemy>().Init(data);
        Debug.Log($"[일반 몬스터] {data.Name} (ID: {data.Id}) 스폰");
    }

    void SpawnElite(int id)
    {
        MonsterData data = monsterDatabase.GetByID(id);

        if (data == null)
        {
            Debug.LogWarning($"[앨리트 몬스터] ID {id}에 해당하는 데이터가 없습니다.");
            return;
        }

        GameObject enemy = GameManager.instance.pool.Get(2); // 앨리트 몬스터는 풀 인덱스2
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        int animIndex = monsterDatabase.monsters.FindIndex(m => m.Id == data.Id);
        if (animIndex >= 0 && animIndex < animCon.Length)
        {
            data.animator = animCon[animIndex];
        }

        enemy.GetComponent<Enemy>().Init(data);
        Debug.Log($"[앨리트 몬스터] {data.Name} (ID: {data.Id}) 등장");
    }

    int GetNormalMonsterIdByTime(float time)
    {
        int minute = Mathf.FloorToInt(time / 60f);
        switch (minute)
        {
            case 0: return 1;
            case 1: return 2;
            case 2: return 3;
            case 3: return 4;
            case 4: return 5;
            case 5: return 6;
            case 6: return 7;
            case 7: return 8;
            case 8: return 9;
            default: return 9;  // 이후 계속 같은 몬스터
        }
    }
}