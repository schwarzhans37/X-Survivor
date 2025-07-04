using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("# Bullet Number")]
    public int id;

    [Header("# Bullet Data")]
    public float damage;    // 탄환 데미지
    public int penetration; // 관통 횟수
    public float bulletSize;    // 탄환 크기

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float damage, int penetration, Vector3 dir)
    {
        this.damage = damage;
        this.penetration  = penetration;

        if (penetration >= 0) {
            rigid.velocity = dir * 15f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || penetration == -100)
            return;

        penetration--;

        if (penetration < 0) {
            rigid.velocity = Vector2.zero;
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
