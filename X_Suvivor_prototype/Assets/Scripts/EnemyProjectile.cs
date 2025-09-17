using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class EnemyProjectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 2f;
    public float lifeTime = 20f;
    public float knockback = 1f;

    Animator anim;
    Collider2D col;
    Vector2 dir;
    float despawnAt;
    bool exploding;

    public void Fire(Vector2 direction, string animName)
    {
        anim ??= GetComponent<Animator>();
        col ??= GetComponent<Collider2D>();

        dir = direction.normalized;
        despawnAt = Time.time + lifeTime;
        exploding = false;
        col.enabled = true;

        // 진행 방향으로 회전(선택)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        anim.Play(animName, 0, 0f);
    }

    void Update()
    {
        if (!exploding)
            transform.position += (Vector3)(dir * speed * Time.deltaTime);

        if (Time.time >= despawnAt && !exploding)
            StartExplode();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploding) return;
        if (!other.CompareTag("Player")) return;

        var p = other.GetComponent<Player>();
        if (p) p.TakeDamage(damage); // 필요하면 넉백 추가

        StartExplode();
    }

    void StartExplode()
    {
        exploding = true;
        col.enabled = false;
        anim.SetTrigger("StartExplode"); // "Explode" 상태로 전환하는 트리거 호출
    }

    // Explode 마지막 프레임에 Animation Event로 호출
    public void Despawn()
    {
        gameObject.SetActive(false);
    }
}
