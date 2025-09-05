using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 10;
    public float knockbackForce = 4f;
    [HideInInspector] public Transform attacker; // 부모(방향 계산용)

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Player 컴포넌트 직접 참조
        var player = other.GetComponent<Player>();
        if (player == null) return;

        // 1) 데미지
        player.TakeDamage(damage);   // Player.cs가 무적/사망 처리까지 알아서 함

        // 2) 넉백 (선택) : Player의 public Rigidbody2D rigid 사용
        if (player.rigid != null && knockbackForce > 0f && attacker != null)
        {
            Vector2 dir = (other.transform.position - attacker.position).normalized;
            player.rigid.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
