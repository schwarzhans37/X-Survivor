using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    /* 
        본 스크립트는 몬스터가 사망할 때 드랍되는 경험치 아이템을 다룬다.
     */

    [Header("# Exp Obejct Data")]
    public int expValue; // 경험치 오브젝트의 경험치 양
    public float speed;   // 플레이어에게 날아가는 속도

    private bool isSeeking; // 플레이어에게 감지된 상태인지
    private Transform player;   // 플레이어의 위치 정보

    void OnEnable()
    {
        // 오브젝트 풀에서 재사용 될 때를 위한 초기화
        isSeeking = false;
        player = GameManager.instance.player.transform;
    }

    void Update()
    {
        if (isSeeking)
        {
            //isSeeking(플레이어 감지 상태)이 true가 되면, 플레이어를 향해 날아감
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    // 플레이어의 감지 범위에 들어왔을 때 호출될 함수
    public void StartSeeking()
    {
        isSeeking = true;
    }

    // 이 오브젝트의 트리거에 다른 콜라이더가 닿았을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        // 닿은 것이 플레이어라면
        if (other.CompareTag("Player"))
        {
            // GameManager에게 경힘치를 상승시키라 전달, 자신은 비활성화
            GameManager.instance.GetExp();
            gameObject.SetActive(false);
        }
    }
}
