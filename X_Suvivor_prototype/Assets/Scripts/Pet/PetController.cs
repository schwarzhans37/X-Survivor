using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;

public class PetController : MonoBehaviour
{
    // 펫의 상태를 명확하게 구분하기 위한 열거형(enum) 정의
    private enum PetState
    {
        Wandering,      // 자유 배회 상태
        Waiting,        // 잠시 자리에서 대기
        Returning       // 플레이어에게 복귀하는 상태
    }

    [Header("펫 설정")]
    [SerializeField] private Transform playerTransform;     // 플레이어의 Transform 인스펙터
    [SerializeField] private float moveSpeed;        // 펫 이동속도
    [SerializeField] private float wanderRadius;     // 플레이어 주변 배화 반경
    [SerializeField] private float returnDistance;   // 반경보다 약간 크게 설정할 것, 거리가 많이 멀어지면 복귀 시작

    [Header("대기 시간 설정")]
    [SerializeField] private float minWaitTime = 0.5f;   // 최소 대기시간
    [SerializeField] private float maxWaitTime = 1.5f;   // 최대 대기시간

    private PetState currentState;          // 펫 현재 상태
    private Vector2 targetPosition;         // 이동 목표 지점
    private Rigidbody2D rigid;              // 물리적 이동을 위한 Rigidbody2D
    private Animator anim;                  // 애니메이션 제어를 위한 Animator
    private SpriteRenderer spriteRenderer;  // 펫의 방향 전환을 위한 SpriteRenderer

    public void Initialize(Transform player)
    {
        this.playerTransform = player;
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 플레이어를 자동으로 찾기 (만약 Inspector에서 할당되지 않았다면)
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogError("플레이어를 찾을 수 없습니다! Player 태그를 확인해주세요.");
                this.enabled = false; // 플레이어가 없으면 스크립트 비활성화
                return;
            }
        }

        // 시작 상태를 '대기'로 설정하고 첫 행동 시작
        currentState = PetState.Waiting;
        StartCoroutine(WaitAndSetNewDestination());
    }

    void Update()
    {
        // 1. 최우선 순위: 플레이어와 너무 멀어졌는지 항상 확인
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > returnDistance)
        {
            // 현재 상태가 이미 '복귀'가 아니라면 상태 변경
            if (currentState != PetState.Returning)
            {
                StopAllCoroutines(); // 진행 중이던 다른 행동(대기 등)을 모두 멈춤
                currentState = PetState.Returning;
                Debug.Log("플레이어와 너무 멀어져 복귀를 시작합니다.");
            }
        }
        else if (currentState == PetState.Returning)
        {
            // 복귀 중 wanderRadius 안으로 들어왔다면, 다시 배회 상태로 전환
            if (distanceToPlayer < wanderRadius * 0.8f) // 약간의 여유를 둠
            {
                StopAllCoroutines();
                currentState = PetState.Waiting;
                StartCoroutine(WaitAndSetNewDestination());
                Debug.Log("플레이어 근처로 복귀 완료. 배회를 재개합니다.");
            }
        }

        // 2. 현재 상태에 따라 행동 처리
        switch (currentState)
        {
            case PetState.Wandering:
                HandleWandering();
                break;
            case PetState.Returning:
                HandleReturning();
                break;
            case PetState.Waiting:
                // 대기 상태에서는 Coroutine이 모든 것을 처리하므로 여기서는 할 일이 없음
                rigid.velocity = Vector2.zero; // 제자리에 멈춤
                break;
        }

        // 애니메이션 및 방향 업데이트
        UpdateAnimationAndDirection();
    }

    // '배회' 상태일 때의 로직
    private void HandleWandering()
    {
        // 목표 지점에 거의 도달했다면
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            rigid.velocity = Vector2.zero; // 정확한 위치에 멈춤
            currentState = PetState.Waiting;
            StartCoroutine(WaitAndSetNewDestination());
        }
        else
        {
            // 목표 지점을 향해 이동
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rigid.velocity = direction * moveSpeed;
        }
    }

    // '복귀' 상태일 때의 로직
    private void HandleReturning()
    {
        // 플레이어를 향해 직접 이동
        targetPosition = playerTransform.position; // 목표를 플레이어로 계속 갱신
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rigid.velocity = direction * moveSpeed;
    }

    // 지정된 시간만큼 대기한 후, 새로운 목표 지점을 설정하는 코루틴
    IEnumerator WaitAndSetNewDestination()
    {
        // 랜덤 대기 시간 설정
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        // 새로운 목표 지점 계산 및 상태 변경
        SetNewWanderDestination();
        currentState = PetState.Wandering;
    }

    // 플레이어 주변에 새로운 배회 목표 지점을 설정하는 함수
    void SetNewWanderDestination()
    {
        // Random.insideUnitCircle은 반지름 1인 원 안의 랜덤한 좌표를 반환
        // 여기에 wanderRadius를 곱해 원하는 반경 내의 랜덤 위치를 얻음
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        targetPosition = (Vector2)playerTransform.position + randomPoint;
        Debug.Log($"새로운 목표 설정: {targetPosition}");
    }

    // 펫의 이동에 따라 애니메이션과 보는 방향(좌/우)을 업데이트
    private void UpdateAnimationAndDirection()
    {
        // Animator에 isMoving 파라미터가 있다고 가정
        if (anim != null)
        {
            // 속도가 0.1보다 크면 움직이는 것으로 간주
            anim.SetBool("isMoving", rigid.velocity.magnitude > 0.1f);
        }

        // 보는 방향 설정
        if (spriteRenderer != null)
        {
            // 오른쪽으로 움직일 때
            if (rigid.velocity.x > 0.05f)
            {
                spriteRenderer.flipX = false;
            }
            // 왼쪽으로 움직일 때
            else if (rigid.velocity.x < -0.05f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    // Scene 뷰에서 배회 반경을 시각적으로 확인하기 위한 Gizmo
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // 배회 반경 (Wander Radius)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, wanderRadius);

            // 복귀 거리 (Return Distance)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, returnDistance);
        }
    }
}