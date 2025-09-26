using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetProjectile : MonoBehaviour
{
    [Header("투사체 능력치")]
    private float speed;
    private float damage;
    private float maxDistance; // 이 투사체가 날아갈 수 있는 최대 거리
    private Transform target;
    private Vector3 initialPosition;

    // 투사체가 발사될 때 펫이 호출해주는 초기화 함수
    public void Init(Transform newTarget, float newDamage, float newMaxDistance)
    {
        this.target = newTarget;
        this.damage = newDamage;
        this.maxDistance = newMaxDistance;
        this.initialPosition = transform.position;
    }

    void Update()
    {
        // 1. 목표물이 사라졌거나 비활성화되었다면 투사체도 제거
        if (target == null || !target.gameObject.activeSelf)
        {
            gameObject.SetActive(false); // 풀로 반환
            return;
        }

        // 2. 목표물을 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 3. 최대 사거리를 벗어났다면 제거
        if (Vector3.Distance(initialPosition, transform.position) > maxDistance)
        {
            gameObject.SetActive(false); // 풀로 반환
        }
    }

    // 4. 적과 충돌했을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy 태그를 가진 오브젝트와 충돌했고, 그 Enemy가 살아있다면
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                // 데미지를 주고 투사체는 풀로 반환
                enemy.ApplyDamage(damage, transform.position, 1f); // 가벼운 넉백
                gameObject.SetActive(false);
            }
        }
    }
}
