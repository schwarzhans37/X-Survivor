using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 카테고리별로 프리팹 배열을 분리합니다.
    [Header("# Prefab Categories")]
    public GameObject[] itemPrefabs;      // 드랍 아이템 (경험치, 골드 등)
    public GameObject[] projectilePrefabs; // 투사체 (총알, 검기 등)
    public GameObject[] normalMonsterPrefabs;    // 일반 몬스터
    public GameObject[] eliteMonsterPrefabs;    // 엘리트 몬스터
    public GameObject[] bossMonsterPrefabs;     // 보스 몬스터
    public GameObject[] petPrefabs;         // 펫 전용 프리팹

    // 각 카테고리별 풀을 담당할 리스트 배열들
    List<GameObject>[] itemPools;
    List<GameObject>[] projectilePools;
    List<GameObject>[] normalMonsterPools;
    List<GameObject>[] eliteMonsterPools;
    List<GameObject>[] bossMonsterPools;
    List<GameObject>[] petPools;

    // 어떤 종류의 풀을 사용할지 구분하기 위한 열거형(enum)
    public enum PoolCategory
    {
        Item,
        Projectile,
        NormalMonster,
        EliteMonster,
        BossMonster,
        Pet
    }

    void Awake()
    {
        // 각 카테고리별 풀 리스트를 초기화합니다.
        itemPools = new List<GameObject>[itemPrefabs.Length];
        for (int i = 0; i < itemPools.Length; i++)
        {
            itemPools[i] = new List<GameObject>();
        }

        projectilePools = new List<GameObject>[projectilePrefabs.Length];
        for (int i = 0; i < projectilePools.Length; i++)
        {
            projectilePools[i] = new List<GameObject>();
        }

        normalMonsterPools = new List<GameObject>[normalMonsterPrefabs.Length];
        for (int i = 0; i < normalMonsterPools.Length; i++)
        {
            normalMonsterPools[i] = new List<GameObject>();
        }

        eliteMonsterPools = new List<GameObject>[eliteMonsterPrefabs.Length];
        for (int i = 0; i < eliteMonsterPools.Length; i++)
        {
            eliteMonsterPools[i] = new List<GameObject>();
        }

        bossMonsterPools = new List<GameObject>[bossMonsterPrefabs.Length];
        for (int i = 0; i < bossMonsterPools.Length; i++)
        {
            bossMonsterPools[i] = new List<GameObject>();
        }

        petPools = new List<GameObject>[petPrefabs.Length];
        for (int i = 0; i < petPools.Length; i++)
        {
            petPools[i] = new List<GameObject>();
        }
    }

    // 어떤 카테고리의 몇 번째 인덱스를 가져올지 명시적으로 요청하는 Get 함수
    public GameObject Get(PoolCategory type, int index)
    {
        List<GameObject>[] targetPool = null;
        GameObject[] targetPrefab = null;

        // 요청된 타입에 따라 사용할 풀과 프리팹 배열을 선택
        switch (type)
        {
            case PoolCategory.Item:
                targetPool = itemPools;
                targetPrefab = itemPrefabs;
                break;
            case PoolCategory.Projectile:
                targetPool = projectilePools;
                targetPrefab = projectilePrefabs;
                break;
            case PoolCategory.NormalMonster:
                targetPool = normalMonsterPools;
                targetPrefab = normalMonsterPrefabs;
                break;
            case PoolCategory.EliteMonster:
                targetPool = eliteMonsterPools;
                targetPrefab = eliteMonsterPrefabs;
                break;
            case PoolCategory.BossMonster:
                targetPool = bossMonsterPools;
                targetPrefab = bossMonsterPrefabs;
                break;
            case PoolCategory.Pet:
                targetPool = petPools;
                targetPrefab = petPrefabs;
                break;
        }

        GameObject select = null;

        // 선택한 풀에서 비활성화된 오브젝트 탐색
        foreach (GameObject item in targetPool[index]) {
            if (!item.activeSelf) {
                select = item;
                select.SetActive(true);
                break;
            }
        }
        
        // 찾지 못했다면 새로 생성
        if (!select) {
            select = Instantiate(targetPrefab[index], transform);
            targetPool[index].Add(select);
        }

        return select;
    }
}