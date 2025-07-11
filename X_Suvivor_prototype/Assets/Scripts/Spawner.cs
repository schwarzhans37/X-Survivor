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

    [System.Serializable]
    public class EliteSchedule
    {
        public int spawnTime;        // �� ����
        public int monsterId;        // ��: 1000
        public bool spawned = false; // �ߺ� ���� ����
    }

    public List<EliteSchedule> eliteSchedule = new List<EliteSchedule>();

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();

        if (monsterDatabase == null || monsterDatabase.monsters.Count == 0)
        {
            Debug.LogError("[Spawner] MonsterDatabase�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }
    }

    void Start()
    {
        if (monsterDatabase == null)
        {
            Debug.LogError("[Spawner] MonsterDatabase�� ������� �ʾҽ��ϴ�.");
            return;
        }

        // ������ �ε��� ���� ������ ��� ��� (���� ���� ����)
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);  // ������ �ε� ��ٸ���
        timer = spawnInterval;  // �ٷ� �����ϵ��� �ʱ�ȭ
    }

    void Update()
    {
        if (!GameManager.instance.isLive || monsterDatabase == null || monsterDatabase.monsters.Count == 0)
            return;

        float currentTime = GameManager.instance.gameTime;
        timer += Time.deltaTime;

        // �Ϲ� ���� ����
        if (timer > spawnInterval)
        {
            timer = 0;
            SpawnNormal(currentTime);
        }

        // ����Ʈ ���� ����
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
            Debug.LogWarning($"[�Ϲ� ����] ID {monsterId}�� �ش��ϴ� �����Ͱ� �����ϴ�.");
            return;
        }

        GameObject enemy = GameManager.instance.pool.Get(0); // �Ϲ� ���ʹ� Ǯ �ε��� 0
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        int animIndex = monsterDatabase.monsters.FindIndex(m => m.Id == data.Id);
        if (animIndex >= 0 && animIndex < animCon.Length)
        {
            data.animator = animCon[animIndex];
        }

        enemy.GetComponent<Enemy>().Init(data);
        Debug.Log($"[�Ϲ� ����] {data.Name} (ID: {data.Id}) ����");
    }

    void SpawnElite(int id)
    {
        MonsterData data = monsterDatabase.GetByID(id);

        if (data == null)
        {
            Debug.LogWarning($"[����Ʈ ����] ID {id}�� �ش��ϴ� �����Ͱ� �����ϴ�.");
            return;
        }

        GameObject enemy = GameManager.instance.pool.Get(3); // ����Ʈ ���ʹ� Ǯ �ε��� 3
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        int animIndex = monsterDatabase.monsters.FindIndex(m => m.Id == data.Id);
        if (animIndex >= 0 && animIndex < animCon.Length)
        {
            data.animator = animCon[animIndex];
        }

        enemy.GetComponent<Enemy>().Init(data);
        Debug.Log($"[����Ʈ ����] {data.Name} (ID: {data.Id}) ����");
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
            default: return 9;  // ���� ��� ���� ����
        }
    }
}
