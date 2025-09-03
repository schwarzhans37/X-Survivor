using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 확률적 스킬 드랍을 인스펙터에서 편하게 설정하기 위한 보조 클래스
[System.Serializable]
public class SkillDrop
{
    public ItemSkillData skillData;
    [Tooltip("이 스킬 아이템이 드랍될 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float dropChance;
}

public class TreasureChest : MonoBehaviour
{
    [Header("상호작용 설정")]
    [Tooltip("상자를 여는 데 필요한 시간 (초)")]
    public float openTime = 2.5f;
    [Tooltip("상자가 열리는 진행 상태를 보여줄 원형 이미지")]
    public Image progressCircle;

    [Header("드랍 아이템 설정")]
    [Tooltip("확정적으로 드랍할 아이템 목록 (골드, 젬 등)")]
    public List<DropItem> fixedDrops; // MonsterData.cs의 DropItem 클래스 재활용
    [Tooltip("확률적으로 드랍할 수 있는 스킬 아이템 목록")]
    public List<SkillDrop> potentialSkillDrops;

    private float currentTimer = 0f;
    private bool isPlayerInside = false;
    private bool isOpened = false;

    private Animator anim;
    private Collider2D coll;

    void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }
    
    void OnEnable()
    {
        isOpened = false;
        coll.enabled = true;
        progressCircle.fillAmount = 0;
        progressCircle.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isOpened) return;

        if (isPlayerInside)
        {
            currentTimer += Time.deltaTime;
            progressCircle.fillAmount = currentTimer / openTime;

            if (currentTimer >= openTime)
            {
                OpenChest();
            }
        }
        else
        {
            if (currentTimer > 0)
            {
                currentTimer -= Time.deltaTime * 2f;
                progressCircle.fillAmount = currentTimer / openTime;
            }
            else
            {
                progressCircle.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpened || !other.CompareTag("Player")) return;
        isPlayerInside = true;
        progressCircle.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isOpened || !other.CompareTag("Player")) return;
        isPlayerInside = false;
    }

    private void OpenChest()
    {
        isOpened = true;
        isPlayerInside = false;
        coll.enabled = false;
        progressCircle.gameObject.SetActive(false);
        anim.SetTrigger("Open");
    }

    // 애니메이션 이벤트에서 호출될 함수
    public void SpawnItems()
    {
        // 1. 확정 드랍 (골드, 젬 등)
        foreach (var itemToDrop in fixedDrops)
        {
            int amount = Random.Range(itemToDrop.minAmount, itemToDrop.maxAmount + 1);
            for (int i = 0; i < amount; i++)
            {
                GameObject item = GameManager.instance.pool.Get(PoolCategory.Item, itemToDrop.itemPoolIndex);
                Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
                item.transform.position = spawnPos;
            }
        }

        // 2. 확률적 스킬 드랍
        foreach (var skillDrop in potentialSkillDrops)
        {
            // 설정된 확률에 따라 드랍 여부 결정
            if (Random.Range(0f, 1f) <= skillDrop.dropChance)
            {
                GameManager.instance.player.GetComponent<SkillInventory>().AddItem(skillDrop.skillData);
                // 스킬 획득 시 시각/청각적 피드백을 주면 더 좋습니다.
            }
        }
    }

    // 애니메이션 이벤트에서 호출될 함수
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}