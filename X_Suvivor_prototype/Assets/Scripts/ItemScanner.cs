using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScanner : MonoBehaviour
{
    [Tooltip("아이템을 감지할 범위")]
    public float scanRange;
    [Tooltip("감지할 아이템의 레이어")]
    public LayerMask targetLayer;

    // 감지된 모든 아이템의 콜러이더를 저장할 배열
    private Collider2D[] targets;

    // 매 프레임 감지된 아이템들을 반환하는 프로퍼티
    public Collider2D[] Targets
    {
        get { return targets; }
    }

    void FixedUpdate()
    {
        targets = Physics2D.OverlapCircleAll(transform.position, scanRange, targetLayer);
    }

    // [개발자 전용] 에디터에서 감지범위를 시각적으로 볼 수 있게 함 
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}