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
    public int penetration; // 관통 횟수
    public float bulletSize;    // 탄환 크기
    public float bulletSpeed;     // 탄환 속도

    private Rigidbody2D rigid;
    private Vector2 moveDir;    // 이동 방향 벡터

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float dmg, int pene, float spd, Vector2 dir)
    {
        damage = dmg;
        penetration = pene;
        bulletSpeed = spd;
        moveDir = dir;

        if (bulletSpeed > 0)
        {
            rigid.velocity = dir * bulletSpeed;
        }
        else // 근접 무기(속도 0)의 경우
        {
            rigid.velocity = Vector2.zero;
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
