using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureChest : MonoBehaviour
{
    [Header("상호작용 설정")]
    [Tooltip("상자를 여는 데 필요한 시간 (초)")]
    public float openTime = 2.5f;
    [Tooltip("상자가 열리는 진행 상태를 보여줄 원형 이미지")]
    public Image progressCircle;

    [Header("드랍 아이템 설정")]
    [Tooltip("드랍할 아이템 목록")]
    public List<DropItem> dropList; // MonsterData.cs에 있던 DropItem 클래스 재활용

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

    // 오브젝트 풀에서 재사용될 때를 대비한 초기화 함수
    void OnEnable()
    {
        isOpened = false;
        coll.enabled = true; // 콜라이더 다시 활성화
        progressCircle.fillAmount = 0; // 진행 원 초기화
        progressCircle.gameObject.SetActive(false); // 처음엔 숨김
        // TODO: 애니메이터를 '닫힌' 상태로 되돌리는 로직 추가 (필요 시)
        // anim.Rebind(); or anim.Play("Idle_State_Name");
    }

    void Update()
    {
        if (isOpened) return; // 이미 열렸으면 아무것도 안 함

        if (isPlayerInside)
        {
            // 플레이어가 안에 있으면 타이머 증가
            currentTimer += Time.deltaTime;
            progressCircle.fillAmount = currentTimer / openTime;

            // 타이머가 다 차면 상자 열기
            if (currentTimer >= openTime)
            {
                OpenChest();
            }
        }
        else
        {
            // 플레이어가 밖에 있으면 타이머 감소 (서서히 사라지는 효과)
            if (currentTimer > 0)
            {
                currentTimer -= Time.deltaTime * 2f; // 더 빨리 감소
                progressCircle.fillAmount = currentTimer / openTime;
            }
            else
            {
                // 타이머가 0이 되면 진행 원 UI를 완전히 숨김
                progressCircle.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpened) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            progressCircle.gameObject.SetActive(true); // 플레이어가 들어오면 진행 원 보이기
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isOpened) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            // 타이머를 바로 0으로 만들면 밖으로 나가자마자 원이 팍 사라짐.
            // 점진적으로 감소시키기 위해 여기서는 타이머를 리셋하지 않음.
        }
    }

    private void OpenChest()
    {
        isOpened = true;
        isPlayerInside = false;
        coll.enabled = false; // 중복 상호작용 방지
        progressCircle.gameObject.SetActive(false); // 열리면 진행 원 숨기기

        anim.SetTrigger("Open"); // "Open"이라는 이름의 트리거 파라미터를 애니메이터에 전달
    }

    // 애니메이션 이벤트에서 호출될 함수 1
    public void SpawnItems()
    {
        foreach (var itemToDrop in dropList)
        {
            if (Random.Range(0f, 1f) <= itemToDrop.dropChance)
            {
                int amount = Random.Range(itemToDrop.minAmount, itemToDrop.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                {
                    GameObject item = GameManager.instance.pool.Get(PoolCategory.Item, itemToDrop.itemPoolIndex);
                    // 상자 주변에 아이템이 흩뿌려지도록 연출
                    Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
                    item.transform.position = spawnPos;
                }
            }
        }
    }

    // 애니메이션 이벤트에서 호출될 함수 2
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
