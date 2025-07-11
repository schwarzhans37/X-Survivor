using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("# 플레이어 캐릭터 능력치")]
    public float speed; //실 적용 이동속도
    public float baseSpeed; // 기본 이동속도
    public float health;    // 현재 체력
    public float maxHealth;   // 최대 체력
    //스킬은 쿨타임제로 , 기초 스텟에 미반영

    [Header("# 플레이어 캐릭터 물리")]
    public Vector2 inputVec;    // 캐릭터 좌표
    public ItemScanner itemScanner;     // 캐릭터의 아이템 회수범위

    [Header("# 캐릭터 이미지 렌더")]
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon; // 플레이어 캐릭터 애니메이션 컨트롤러

    Rigidbody2D rigid;  // 충돌 판정 계산 리지드바디
    SpriteRenderer spriter; // 스프라이트(이미지)
    Animator anim;  // 캐릭터 애니메이션


    void Awake()
    // 초기화(선언)
    {
        speed = baseSpeed;      // 시작 시 이동속도 초기화
        health = maxHealth;     // 시작 시 체력 초기화
        
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        itemScanner = GetComponent<ItemScanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnEnable()
    // 오브젝트가 활성화 되었을 때 마다 호출함
    {
        speed = baseSpeed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    
    void FixedUpdate()
    /* 
        물리(Physics)계산용 업데이트 함수
        플레이어의 위치 이동을 위해 사용됨
        - 프레임 속도 영향 안받음(컴퓨터 성능 영향 안받음)
        - 고정된 시간 간격으로 호출함
        - Rigidbody
     */
    {
        if (!GameManager.instance.isLive) {
            return;
        }

        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        // 위치 이동
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    /* 
        한 프레임의 모든 Update 함수가 실행 된 후 호출
        오브젝트의 이동, 로직 등의 처리가 끝난 최종 결과를 가지고 사용
        - 프레임 속도에 따라 가변적임
        - 애니메이션 후처리, 카메라 추적 등 사용
     */
    {
        // 애니메이션 플립 로직
        anim.SetFloat("speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }

        // ItemScanner의 아이템 흡수 로직
        if (itemScanner != null && itemScanner.Targets.Length > 0)
        {
            // 모든 감지된 아이템에 대해 반복
            foreach (Collider2D target in itemScanner.Targets)
            {
                ExpOrb expOrb = target.GetComponent<ExpOrb>();
                if (expOrb != null)
                {
                    expOrb.StartSeeking();
                }
            }

        }
    }

    void OnCollisionStay2D(Collision2D collision)
    // 캐릭터가 몬스터와 충돌할 경우 데미지를 받게 함
    {
        // 몬스터 외의 오브젝트(아이템, 경험치 등)와 충돌했을 경우는 제외
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        TakeDamage(Time.deltaTime * 10);
    }

    public void TakeDamage(float damage)
    {
        if (!GameManager.instance.isLive) return;

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            Die();  //캐릭터 사망 시 죽음 처리 함수 호출
        }
    }

    void Die()
    // 플레이어 사망 시 내부 처리 로직
    {  
        for (int index = 2; index < transform.childCount; index++)
        {
            transform.GetChild(index).gameObject.SetActive(false);
        }

        // 플레이어 캐릭터 사망 애니메이션 활성화
        anim.SetTrigger("dead");

        // 게임 매니저에게 '사망했음'을 알림
        GameManager.instance.GameOver();
    }

    public void Heal()
    {
        health = maxHealth;
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }


}
