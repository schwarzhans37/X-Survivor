using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPUI : MonoBehaviour
{
    public Slider hpSlider;      // 보스 체력 바 (인스펙터에서 연결)
    public Text bossNameText;    // 보스 이름 텍스트 (인스펙터에서 연결)
    public GameObject container;

    private Enemy currentBoss;   // 현재 추적중인 보스 Enemy 컴포넌트

    void Start()
    {
        Debug.Log("<color=cyan><b>[BossHPUI] Start 함수 실행! GameManager 이벤트 등록을 시도합니다.</b></color>");
        GameManager.instance.OnBossSpawned += HandleBossSpawned;
        GameManager.instance.OnBossDefeated += HandleBossDefeated;

        // 게임 시작 시에는 UI를 숨겨둡니다.
        container.SetActive(false);
    }

    void OnDisable()
    {
        // UI가 비활성화될 때 등록했던 함수들을 해제(구독 취소). 메모리 누수 방지를 위해 필수!
        if (GameManager.instance != null)
        {
            GameManager.instance.OnBossSpawned -= HandleBossSpawned;
            GameManager.instance.OnBossDefeated -= HandleBossDefeated;
        }
    }

    // "보스가 등장했다"는 신호를 받았을 때 실행될 함수
    void HandleBossSpawned(Enemy boss)
    {
        Debug.Log("<color=lime><b>[BossHPUI] HandleBossSpawned 함수 호출됨! UI를 활성화합니다.</b></color>");

        currentBoss = boss; // 전달받은 보스 정보를 저장
        bossNameText.text = boss.monsterData.Name; // 보스 데이터에서 이름을 가져와 UI에 표시

        // 슬라이더의 최대값과 현재값을 보스의 체력에 맞게 초기화
        hpSlider.maxValue = boss.maxHealth;
        hpSlider.value = boss.health;

        container.SetActive(true); // UI 전체를 활성화
    }

    // "보스가 죽었다"는 신호를 받았을 때 실행될 함수
    void HandleBossDefeated()
    {
        currentBoss = null; // 보스 정보 초기화
        container.SetActive(false); // UI 전체를 비활성화
    }

    void LateUpdate()
    {
        // 추적중인 보스가 존재하고, 아직 살아있을 때만 체력 바를 업데이트
        if (currentBoss != null && currentBoss.health > 0)
        {
            hpSlider.value = currentBoss.health;
        }
    }
}
