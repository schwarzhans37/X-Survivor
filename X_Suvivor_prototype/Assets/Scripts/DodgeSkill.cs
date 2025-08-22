using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DodgeSkill : MonoBehaviour
{
    [Header("회피 능력치")]
    public float distance = 3f;  // 돌진 거리
    public float duration = 0.2f; // 돌진에 걸리는 시간
    public float cooldown = 8f;   // 쿨타임

    private Player player; // Player 스크립트 참조
    private float cooldownTimer = 0f; // 남은 쿨타임을 추적
    public float CooldownTimer { get { return cooldownTimer; } }
    public float MaxCooldown { get { return cooldown; } }
    private bool isDashing = false;   // 현재 돌진 중인지 여부

    void Awake()
    {
        // 같은 게임 오브젝트에 있는 Player 컴포넌트를 가져옴
        player = GetComponent<Player>();
    }

    void Update()
    {
        // 쿨타임 계산
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    // Player Input 컴포넌트가 호출할 함수
    public void OnDodge(InputValue value)
    {
        if (player.isDashing || cooldownTimer > 0)
        {
            return;
        }
        StartCoroutine(DodgeCoroutine());
    }

    private IEnumerator DodgeCoroutine()
    {
        player.isDashing = true;
        cooldownTimer = cooldown;

        // Player.cs에 1초 무적을 요청
        player.BecomeInvincible(1.0f);

        // 마우스 방향 계산
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dir = (worldMousePos - player.transform.position).normalized;

        // 이동 처리 (Player의 Rigidbody를 사용)
        float elapsedTime = 0f;
        Vector2 startPos = player.rigid.position;
        Vector2 targetPos = startPos + dir * distance;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            player.rigid.MovePosition(Vector2.Lerp(startPos, targetPos, progress));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.rigid.MovePosition(targetPos); // 위치 보정
        player.isDashing = false;
    }
}
