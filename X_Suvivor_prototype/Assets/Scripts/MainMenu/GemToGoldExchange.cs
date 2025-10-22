using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemToGoldExchange : MonoBehaviour
{
    // 교환 비율은 동일합니다.
    private const long GEM_PER_EXCHANGE = 100L;
    private const long GOLD_PER_EXCHANGE = 10000L;

    [Header("UI 요소 참조")]
    public Text currentExchangeAmount;
    public Text requiredGemText; // 골드 대신 '필요한 젬'을 표시할 Text
    public Button plusButton;
    public Button reduceButton;
    public Button acceptButton;

    private int currentExchangeCount = 1;   // 현재 교환할 횟수 (N값)
    private Color defaultTextColor;

    void Awake()
    {
        // 스크립트가 처음 활성화될 때 Text 컴포넌트의 초기 색상을 저장
        if (requiredGemText != null)
        {
            defaultTextColor = requiredGemText.color;
        }
    }

    void OnEnable()
    {
        // 패널이 활성화 될 때 UI 초기화 및 이벤트 리스너 추가
        InitializeUI();
        plusButton.onClick.AddListener(OnPlusButtonClick);
        reduceButton.onClick.AddListener(OnReduceButtonClick);
        acceptButton.onClick.AddListener(OnAcceptButtonClick);
    }

    void OnDisable()
    {
        // 패널이 비활성화 될 때 이벤트 리스너 제거
        plusButton.onClick.RemoveListener(OnPlusButtonClick);
        reduceButton.onClick.RemoveListener(OnReduceButtonClick);
        acceptButton.onClick.RemoveListener(OnAcceptButtonClick);
    }

    private void InitializeUI()
    {
        currentExchangeCount = 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        currentExchangeAmount.text = currentExchangeCount.ToString() + " 개";

        // '젬' 기준으로 로직 변경
        long requiredGems = currentExchangeCount * GEM_PER_EXCHANGE;
        if (requiredGemText != null)
        {
            requiredGemText.text = requiredGems.ToString("N0") + " 젬 필요";

            long availableGems = PlayerDataManager.instance.playerData.gems;
            if (requiredGems > availableGems)
            {
                requiredGemText.color = Color.red; // 부족하면 빨간색
            }
            else
            {
                requiredGemText.color = defaultTextColor; // 충분하면 원래 색
            }
        }
        UpdateButtonsInteractable();
    }

    private void UpdateButtonsInteractable()
    {
        // '젬' 기준으로 로직 변경
        long availableGems = PlayerDataManager.instance.playerData.gems;
        long requiredGems = currentExchangeCount * GEM_PER_EXCHANGE;

        // Plus 버튼은 보유 젬이 충분하고, int.MaxValue를 초과하지 않을 때만 활성화
        plusButton.interactable = (availableGems >= (long)(currentExchangeCount + 1) * GEM_PER_EXCHANGE) && (currentExchangeCount < int.MaxValue);

        // Reduce 버튼은 최소 수량(1) 이상일 때만 활성화
        reduceButton.interactable = (currentExchangeCount > 1);

        // Accept 버튼은 최소 수량(1) 이상이고, 보유 젬이 충분할 때만 활성화
        acceptButton.interactable = (currentExchangeCount >= 1 && availableGems >= requiredGems);
    }

    public void OnPlusButtonClick()
    {
        // '젬' 기준으로 로직 변경
        long maxPossibleExchange = PlayerDataManager.instance.playerData.gems / GEM_PER_EXCHANGE;
        if (currentExchangeCount < maxPossibleExchange)
        {
            currentExchangeCount++;
        }
        else
        {
            Debug.Log("더 이상 교환할 젬이 부족합니다.");
        }
        UpdateUI();
    }

    public void OnReduceButtonClick()
    {
        if (currentExchangeCount > 1)
        {
            currentExchangeCount--;
        }
        UpdateUI();
    }

    public void OnAcceptButtonClick()
    {
        // '젬' 기준으로 로직 변경
        long totalGemsCost = currentExchangeCount * GEM_PER_EXCHANGE;
        long totalGoldGain = currentExchangeCount * GOLD_PER_EXCHANGE;

        // PlayerDataManager의 SpendGems 메서드를 활용하는 것이 더 안정적입니다.
        if (PlayerDataManager.instance.SpendGems(totalGemsCost))
        {
            // SpendGems가 성공했다면 (젬이 충분했다면) 골드를 추가합니다.
            PlayerDataManager.instance.AddGold(totalGoldGain);
            Debug.Log($"{totalGemsCost} 젬을 사용하여 {totalGoldGain} 골드로 교환했습니다.");

            // 교환 후 다시 1개로 초기화
            currentExchangeCount = 1;
            UpdateUI();
        }
        else
        {
            Debug.LogError("젬이 부족하여 교환할 수 없습니다.");
            // 여기에 재화 부족 메시지 UI 등을 띄우는 로직 추가 가능
        }
    }
}
