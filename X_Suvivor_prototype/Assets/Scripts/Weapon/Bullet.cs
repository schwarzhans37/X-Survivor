using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Analytics;

public class Bullet : MonoBehaviour
{
    [Header("# Bullet Number")]
    public int id;

    [Header("# Bullet Data")]
    public float damage;    // 탄환 데미지
    public float range; //  이 투사체의 최대 사거리
    public int penetration; // 관통 횟수
    public float bulletSize;    // 탄환 크기
    public float bulletSpeed;     // 탄환 속도

    private Rigidbody2D rigid;
    private Vector2 moveDir;    // 이동 방향 벡터
    private Vector3 startPosition;  // 발사된 시작 위치

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float dmg, float rng, int pene, float spd, Vector2 dir)
    {
        damage = dmg;
        range = rng;
        penetration = pene;
        bulletSpeed = spd;
        moveDir = dir;
        startPosition = transform.position;

        if (bulletSpeed > 0)
        {
            rigid.velocity = dir * bulletSpeed;
        }
        else // 근접 무기(속도 0)의 경우
        {
            rigid.velocity = Vector2.zero;
        }
    }

    void Update()
    {
        // 사거리가 0 이하면 무한으로 간주 (안전장치 및 특수 기능용)
        if (range <= 0) return;

        // 시작 위치로부터 현재 위치까지의 거리를 계산
        float distance = Vector3.Distance(startPosition, transform.position);

        // 계산된 거리가 최대 사거리를 넘어서면 비활성화
        if (distance > range)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        // 풀로 돌아갈 때 초기화
        rigid.velocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || penetration == -100)
            return;

        penetration--;

        if (penetration < 0) {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || penetration == -100)
            return;

        gameObject.SetActive(false);
    }
}
