using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private float damage;
    private float explosionRadius;
    private float speed;
    private Vector2 direction;
    private float travelDistance;
    private Vector3 startPos;
    private float knockback = 5f; // 유탄의 넉백 값 (WeaponData에서 받아오도록 확장 가능)

    // Explosion 프리팹을 인스펙터에서 연결
    public GameObject explosionPrefab;

    // GrenadeLauncher로부터 데이터를 받아 초기화
    public void Init(float dmg, float radius, float spd, float distance, Vector2 dir)
    {
        damage = dmg;
        explosionRadius = radius;
        speed = spd;
        travelDistance = distance;
        direction = dir;
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        if (Vector3.Distance(startPos, transform.position) >= travelDistance)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        Transform explosionTransform = GameManager.instance.pool.Get(PoolManager.PoolCategory.Projectile, 12).transform; 
        explosionTransform.position = transform.position;
        
        ExplosionController explosionController = explosionTransform.GetComponent<ExplosionController>();
        if (explosionController != null)
        {
            explosionController.Init(damage, knockback);
        }

        gameObject.SetActive(false);
    }
}
