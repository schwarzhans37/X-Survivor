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
    private bool isDead;                    // 펫이 체력이 다해 넉다운 상태인지 판단

    [Header("대기 시간 설정")]
    [SerializeField] private float minWaitTime = 0.5f;   // 최소 대기시간
    [SerializeField] private float maxWaitTime = 1.5f;   // 최대 대기시간

    [Header("펫 공격 설정")]
    public float attackRange;      // 투사체 공격 사거리
    public float attackDamage;     // 공격 데미지
    public float attackCooldown;  // 공격 쿨타임
    public LayerMask enemyLayer;        // 적만 감지하는 레이어
    private PoolManager poolManager;

    private PetState currentState;          // 펫 현재 상태
    private Vector2 targetPosition;         // 이동 목표 지점
    private Rigidbody2D rigid;              // 물리적 이동을 위한 Rigidbody2D
    private Animator anim;                  // 애니메이션 제어를 위한 Animator
    private SpriteRenderer spriteRenderer;  // 펫의 방향 전환을 위한 SpriteRenderer

    // 코루틴은 변수로 저장하여 각각 관리
    private Coroutine movementCoroutine;
    private Coroutine attackCoroutine;

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

        // PoolManager 인스턴스 찾기
        poolManager = FindObjectOfType<PoolManager>();

        // 시작 상태를 '대기'로 설정하고 첫 행동 시작
        currentState = PetState.Waiting;

        // 이동 코루틴과 공격 코루틴을 각각 시작
        movementCoroutine = StartCoroutine(MovementRoutine());
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        UpdateAnimationAndDirection();

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > returnDistance && currentState != PetState.Returning)
        {
            Debug.Log("플레이어와 너무 멀어져 복귀를 시작합니다.");
            currentState = PetState.Returning;
        }
        else if (currentState == PetState.Returning && distanceToPlayer < wanderRadius * 0.8f)
        {
            Debug.Log("플레이어 근처로 복귀 완료. 배회를 재개합니다.");
            currentState = PetState.Waiting;
            rigid.velocity = Vector2.zero;
        }
    }

    // 이동 관련 로직을 하나의 코루틴으로 관리
    private IEnumerator MovementRoutine()
    {
        while (!isDead) // 펫이 살아있는 동안 계속 반복
        {
            switch (currentState)
            {
                case PetState.Wandering:
                    // 목표 지점으로 이동
                    Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                    rigid.velocity = direction * moveSpeed;

                    // 목표 지점에 도착하면 대기 상태로 변경
                    if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                    {
                        rigid.velocity = Vector2.zero;
                        currentState = PetState.Waiting;
                    }
                    break;

                case PetState.Waiting:
                    // 제자리에 멈춤
                    rigid.velocity = Vector2.zero;

                    // 임의의 시간 동안 대기
                    float waitTime = Random.Range(minWaitTime, maxWaitTime);
                    yield return new WaitForSeconds(waitTime);

                    // 대기 후 새로운 목표 설정 및 배회 시작
                    SetNewWanderDestination();
                    currentState = PetState.Wandering;
                    break;

                case PetState.Returning:
                    // 플레이어를 향해 이동
                    targetPosition = playerTransform.position;
                    Vector2 returnDirection = (targetPosition - (Vector2)transform.position).normalized;
                    rigid.velocity = returnDirection * moveSpeed;
                    break;
            }
            yield return null;  // 매 프레임마다 상태 체크를 위해 루프를 한 번씩 끊기
        }
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

    // 공격 코루틴
    private IEnumerator AttackRoutine()
    {
        while (!isDead)
        {
            // 공격 쿨타임만큼 대기
            yield return new WaitForSeconds(attackCooldown);

            // 가까운 적 찾기
            Transform closestEnemy = FindClosestEnemy();

            // 찾았다면 공격
            if (closestEnemy != null)
            {
                PetAttack(closestEnemy);
            }
        }
    }

    private Transform FindClosestEnemy()
    {
        // attackRange 반경 내의 모든 적 콜라이더 가져옴
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        // 가져온 모든 적들을 순화혀며 가장 가까운 적을 찾음
        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            // 살아있는 적인지 확인
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                Vector3 directionToTarget = enemyCollider.transform.position - currentPosition;

                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = enemyCollider.transform;
                }
            }
        }

        return bestTarget;
    }

    private void PetAttack(Transform target)
    {
        // PoolManager 에게서 펫 투사체를 가져옴
        GameObject nomalAttackPrefab = poolManager.Get(PoolManager.PoolCategory.Pet, 0);
        nomalAttackPrefab.transform.position = transform.position;  // 펫의 위치에서 발사

        // 투사체 효과
        PetProjectile projectile = nomalAttackPrefab.GetComponent<PetProjectile>();
        if (projectile != null)
        {
            // 목표, 데미지, 사거리 전달
            projectile.Init(target, attackDamage, attackRange);
        }
    }
}