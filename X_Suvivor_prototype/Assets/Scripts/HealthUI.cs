using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [Tooltip("체력을 나타내는 하트 프리팹")]
    public GameObject heartPrefab;
    [Tooltip("하트 프리팹이 생성&보관될 부모 오브젝트")]
    public Transform container;

    private List<GameObject> hearts = new List<GameObject>();
    private Player player;
    private int lastHealth; // 마지막으로 기억된 체력을 저장할 변수
    private int lastMaxHealth; // 마지막으로 기억된 '최대 체력'을 저장할 변수

    void Start()
    {
        player = GameManager.instance.player;
        InitializeHearts();
        if (player != null)
        {
            lastMaxHealth = player.maxHealth;
            lastHealth = player.health; //초기 체력 저장
            InitializeHearts();
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        //1. 최대 체력이 변경되었는지 먼저 확인
        if (player.maxHealth != lastMaxHealth)
        {
            // 최대 체력이 바뀌었으므로 하트 UI를 처음부터 다시 그림
            InitializeHearts();
            // 최신 값으로 업데이트
            lastMaxHealth = player.maxHealth;
            lastHealth = player.health;
        }

        // 2. 최대 체력에 변화가 없을 때만, 현재 체력 변화를 확인
        else if (player.health != lastHealth)
        {
            // 현재 체력만 바뀌었다면 기존 하트를 켜고 끄기만 함
            UpdateHearts();
            // 최신 값으로 업데이트
            lastHealth = player.health;
        }
    }

    public void InitializeHearts()
    {
        foreach (var heart in hearts)
        {
            Destroy(heart);
        }
        hearts.Clear();

        for (int i = 0; i < player.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, container);
            hearts.Add(newHeart);
        }
        UpdateHearts();
    }

    // 현재 생명에 맞춰 하트를 끄고 켬
    public void UpdateHearts()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            // 현재 생명보다 인덱스가 크거나 같으면(즉, 생명을 잃어버리면) 비활성화
            if (i >= player.health)
            {
                hearts[i].SetActive(false);
            }
            else
            {
                hearts[i].SetActive(true);
            }
        }
    }
}
