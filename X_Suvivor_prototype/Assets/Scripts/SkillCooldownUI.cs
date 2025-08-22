using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("연결할 대상")]
    [Tooltip("쿨타임 시간을 표시할 텍스트")]
    public DodgeSkill dodgeSkill;

    [Header("UI 컴포넌트")]
    [Tooltip("쿨타임 시간을 표시할 텍스트")]
    public Text cooldownText;
    [Tooltip("스킬 아이콘 이미지")]
    public Image iconImage;
    [Tooltip("버튼 컴포넌트 (비활성화 효과용)")]
    public Button skillButton;

    void Start()
    {
        // 게임 시작 시 쿨타임 텍스트는 보이지 않게 설정
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // dodgeSkill이 연결되지 않았으면 아무것도 하지 않음
        if (dodgeSkill == null) return;

        // dodgeSkill 스크립트에서 현재 쿨타임 정보를 가져옴
        float remainingCooldown = dodgeSkill.CooldownTimer;

        if (remainingCooldown > 0)
        {
            // 쿨타임이 남았을 경우
            skillButton.interactable = false;
            cooldownText.gameObject.SetActive(true);
            cooldownText.text = remainingCooldown.ToString("F1");   // 소수점 한 자리까지 표시
        }
        else
        {
            // 쿨타임이 끝났을 경우
            skillButton.interactable = true;
            if (cooldownText.gameObject.activeSelf)
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }
}
