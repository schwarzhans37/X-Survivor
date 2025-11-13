using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldToGemExchange : MonoBehaviour
{
    private const long GOLD_PER_EXCHANGE = 10000L;
    private const long GEM_PER_EXCHANGE = 100L;

    [Header("UI 요소 참조")]
    public Text currentExchangeAmount;
    public Text requiredGoldText;
    public Button plusButton;
    public Button reduceButton;
    public Button acceptButton;

    private int currentExchangeCount = 1;   // 현재 교환할 횟수 (N값)
    private Color defaultTextColor;

    void Awake()
    {
        // 스크립트가 처음 활성화될 때 Text 컴포넌트의 초기 색상을 저장
        if (requiredGoldText != null)
        {
            defaultTextColor = requiredGoldText.color;
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
        UpdateButtonsInteractable();

        long requiredGold = currentExchangeCount * GOLD_PER_EXCHANGE;
        if (requiredGoldText != null)
        {
            requiredGoldText.text = requiredGold.ToString("N0") + " 골드 필요";

            long availableGold = PlayerDataManager.instance.playerData.gold;
            if (requiredGold > availableGold)
            {
                requiredGoldText.color = Color.red;
            }
            else
            {
                requiredGoldText.color = defaultTextColor;
            }
        }
        UpdateButtonsInteractable();
    }

    private void UpdateButtonsInteractable()
    {
        long availableGold = PlayerDataManager.instance.playerData.gold;
        long requiredGold = currentExchangeCount * GOLD_PER_EXCHANGE;

        // Plus 버튼은 보유 골드가 충분하고, int.MaxValue를 초과하지 않을 때만 활성화
        plusButton.interactable = (availableGold >= (currentExchangeCount + 1) * GOLD_PER_EXCHANGE) && (currentExchangeCount < int.MaxValue);

        // Reduce 버튼은 최소 수량(1) 이상일 때만 활성화
        reduceButton.interactable = (currentExchangeCount > 1);

        // Accept 버튼은 최소 수량(1) 이상이고, 보유 골드가 충분할 때만 활성화
        acceptButton.interactable = (currentExchangeCount >= 1 && availableGold >= requiredGold);
    }

    public void OnPlusButtonClick()
    {
        // 플레이어의 현재 골드와 교환 가능한 최대 횟수를 고려하여 증가
        long maxPossibleExchange = PlayerDataManager.instance.playerData.gold / GOLD_PER_EXCHANGE;
        if (currentExchangeCount < maxPossibleExchange)
        {
            currentExchangeCount++;
        }
        else
        {
            // 차후 시각적인 연출 보강 필요
            Debug.Log("더 이상 교환할 골드가 부족합니다.");
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
        long totalGoldCost = currentExchangeCount * GOLD_PER_EXCHANGE;
        long totalGemsGain = currentExchangeCount * GEM_PER_EXCHANGE;

        if (PlayerDataManager.instance.playerData.gold >= totalGoldCost)
        {
            // 보유 골드 차감 및 젬 추가
            PlayerDataManager.instance.playerData.gold -= totalGoldCost;
            PlayerDataManager.instance.playerData.gems += totalGemsGain;
            PlayerDataManager.instance.SaveData();
            PlayerDataManager.instance.NotifyDataUpdate();
            Debug.Log($"{totalGoldCost} 골드를 사용하여 {totalGemsGain} 젬으로 교환했습니다.");

            // 교환 후 다시 1개로 초기화
            currentExchangeCount = 1;
            UpdateUI();
        }
        else
        {
            Debug.LogError("골드가 부족하여 교환할 수 없습니다.");
            // 여기에 재화 부족 메시지 UI 등을 띄우는 로직 추가 가능
        }
    }
}