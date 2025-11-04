using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSkillSlotUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    [Tooltip("남은 쿨타임을 표시할 텍스트")]
    public Text cooldownText; // CoolTime 게임 오브젝트를 여기에 연결하세요.

    private Button skillButton;
    private Image skillButtonImage; // 버튼의 이미지를 제어하기 위한 변수
    private PetSkillRunner currentSkillRunner;

    void Awake()
    {
        skillButton = GetComponent<Button>();
        skillButtonImage = GetComponent<Image>(); // Image 컴포넌트를 가져옵니다.
        
        // 게임 시작 시에는 UI를 보이지 않게 초기화합니다.
        ClearSlot();
    }

    void Update()
    {
        if (currentSkillRunner == null)
        {
            return;
        }

        float remainingCooldown = currentSkillRunner.CooldownRemaining;
        bool onCooldown = remainingCooldown > 0f;

        skillButton.interactable = !onCooldown;
        cooldownText.gameObject.SetActive(onCooldown);

        if (onCooldown)
        {
            cooldownText.text = remainingCooldown.ToString("F1");
        }
    }

    /// <summary>
    /// PetSkillInstaller가 이 함수를 호출하여 스킬 로직(Runner)과 UI를 연결합니다.
    /// </summary>
    public void LinkSkillRunner(PetSkillRunner runner)
    {
        currentSkillRunner = runner;

        // UI를 활성화하고 버튼 이미지를 다시 보이게 합니다.
        if (skillButtonImage != null)
        {
            skillButtonImage.enabled = true;
        }
        skillButton.interactable = true;
        cooldownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 펫이 스킬을 가지고 있지 않을 때 UI를 숨깁니다.
    /// </summary>
    public void ClearSlot()
    {
        currentSkillRunner = null;
        
        // gameObject를 비활성화하는 대신, 이미지만 보이지 않게 처리합니다.
        if (skillButtonImage != null)
        {
            skillButtonImage.enabled = false;
        }
        skillButton.interactable = false;
        cooldownText.gameObject.SetActive(false);
    }
}
