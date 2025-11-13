using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [Header("플레이어 체력 UI")]
    [Tooltip("체력을 나타내는 하트 프리팹")]
    public GameObject heartPrefab;
    [Tooltip("하트 프리팹이 생성&보관될 부모 오브젝트")]
    public Transform container;

    [Header("펫 체력 UI")] // [추가] 펫 UI를 위한 인스펙터 섹션
    [Tooltip("펫 체력을 나타내는 하트 프리팹")]
    public GameObject petHeartPrefab;
    [Tooltip("펫 하트 프리팹이 생성&보관될 부모 오브젝트")]
    public Transform p_container;

    private List<GameObject> hearts = new List<GameObject>();
    private List<GameObject> p_hearts = new List<GameObject>();
    
    private Player player;
    private PetController pet; // 펫 컨트롤러 참조

    // 플레이어 체력 변화 감지를 위한 변수
    private int lastHealth;
    private int lastMaxHealth;

    // 펫 체력 변화 감지를 위한 변수
    private int lastPetHealth;
    private int lastPetMaxHealth;
    private bool wasPetAlive = false; // 이전 프레임에서 펫의 생존 상태

    void Start()
    {
        player = GameManager.instance.player;
        if (player != null)
        {
            lastMaxHealth = player.maxHealth;
            lastHealth = player.health; //초기 체력 저장
            InitializeHearts();
        }

        if (p_container != null)
        {
            p_container.gameObject.SetActive(false);
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

        if (pet == null)
        {
            pet = FindObjectOfType<PetController>();
            // 펫을 처음 찾았다면, 체력 UI를 초기화합니다.
            if (pet != null)
            {
                InitializePetHearts();
            }
        }
        
        if (pet != null)
        {
            // 펫이 죽었다가 살아났는지 확인
            if (!wasPetAlive && pet.IsAlive)
            {
                // 부활 시 UI를 다시 초기화
                InitializePetHearts();
            }
            // 펫이 살아있을 때만 체력 변화를 감지
            else if (pet.IsAlive)
            {
                // 펫의 최대 체력이 변경되었는지 확인 (강화 등)
                if (pet.currentMaxHealth != lastPetMaxHealth) // PetController의 public 변수 이름 확인 필요
                {
                    InitializePetHearts();
                }
                // 펫의 현재 체력이 변경되었는지 확인
                else if (pet.currentHealth != lastPetHealth) // PetController의 현재 체력 접근 방법 확인 필요, 임시로 TakeDamage(0) 사용
                {
                    UpdatePetHearts();
                }
            }
            // 펫이 죽었는지 확인
            else if (wasPetAlive && !pet.IsAlive)
            {
                 // 죽었으면 모든 하트를 끕니다.
                 UpdatePetHearts();
            }

            // 현재 펫의 생존 상태를 다음 프레임을 위해 저장
            wasPetAlive = pet.IsAlive;
        }
    }

    #region Player Hearts
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
    #endregion

    #region Pet Hears
    public void InitializePetHearts()
    {
        if (p_container == null || petHeartPrefab == null || pet == null) return;

        // 펫 UI 컨테이너 활성화
        p_container.gameObject.SetActive(true);

        foreach (var heart in p_hearts)
        {
            Destroy(heart);
        }
        p_hearts.Clear();

        // [수정] PetController의 변수 이름에 맞춰 수정합니다.
        lastPetMaxHealth = pet.currentMaxHealth;

        for (int i = 0; i < lastPetMaxHealth; i++)
        {
            GameObject newHeart = Instantiate(petHeartPrefab, p_container);
            p_hearts.Add(newHeart);
        }
        UpdatePetHearts();
    }
    
    public void UpdatePetHearts()
    {
        if (pet == null) return;
        
        // [수정] PetController의 현재 체력 접근 방법 확인 필요
        int petCurrentHealth = pet.IsAlive ? pet.currentHealth : 0; // 임시 접근
        lastPetHealth = petCurrentHealth;

        for (int i = 0; i < p_hearts.Count; i++)
        {
            if (i >= petCurrentHealth)
            {
                p_hearts[i].SetActive(false);
            }
            else
            {
                p_hearts[i].SetActive(true);
            }
        }
    }
    #endregion
}
