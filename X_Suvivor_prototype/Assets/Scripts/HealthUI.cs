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

    void Start()
    {
        player = GameManager.instance.player;
        InitializeHearts();
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
