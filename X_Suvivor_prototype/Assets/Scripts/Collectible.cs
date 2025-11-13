using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Experience, Gold, Gem }

    [Header("드랍 아이템 정보")]
    public CollectibleType type;    // 인스펙터에서 설정하는 아이템 종류
    public int value;               // 아이템의 가치 (경험치량, 골드량 등)
    public float speed = 10;        // 플레이어에게 날아가는 속도;

    private bool isSeeking;         // 플레이어에게 감지된 상태인가
    private Transform player;       // 플레이어의 위치정보

    [Header("Audio")]
    public string expSfxKey = "Exp";
    public string goldSfxKey = "Money";
    public string gemSfxKey = "Money";
    [Range(0f, 3f)] public float sfxVolume = 1f;
    [Range(0.1f, 3f)] public float sfxPitch = 1f;

    [Header("같은 소리 과다 재생 방지(최소 간격)")]
    public float minExpSfxInterval = 0.04f;
    public float minGoldSfxInterval = 0.06f;
    public float minGemSfxInterval = 0.06f;

    static float _lastExpSfxTime;
    static float _lastGoldSfxTime;
    static float _lastGemSfxTime;

    void OnEnable()
    {
        // 오브젝트 풀링에서 재사용 될 때를 위한 초기화
        isSeeking = false;
        player = GameManager.instance.player.transform;
    }

    void Update()
    {
        if (isSeeking)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    // 플레이어의 아이템 스캐너에 감지되었을 때 호출될 함수
    public void StartSeeking()
    {
        isSeeking = true;
    }

    // 이 오브젝트의 트리거에 다른 콜라이더가 닿았을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 아이템의 종류(type)에 따라 GameManager의 다른 함수를 호출
            switch (type)
            {
                case CollectibleType.Experience:
                    GameManager.instance.GetExp(value);
                    break;
                case CollectibleType.Gold:
                    GameManager.instance.GetGold(value);
                    break;
                case CollectibleType.Gem:
                    GameManager.instance.GetGems(value);
                    break;
            }

            // 사운드 재생
            PlayPickupSfx();

            gameObject.SetActive(false);
        }
    }
    void PlayPickupSfx()
    {
        if (AudioManager.instance == null) return;

        switch (type)
        {
            case CollectibleType.Experience:
                if (Time.time - _lastExpSfxTime < minExpSfxInterval) return;
                _lastExpSfxTime = Time.time;
                if (!string.IsNullOrEmpty(expSfxKey))
                    AudioManager.instance.PlaySfx(expSfxKey, sfxVolume, sfxPitch);
                break;

            case CollectibleType.Gold:
                if (Time.time - _lastGoldSfxTime < minGoldSfxInterval) return;
                _lastGoldSfxTime = Time.time;
                if (!string.IsNullOrEmpty(goldSfxKey))
                    AudioManager.instance.PlaySfx(goldSfxKey, sfxVolume, sfxPitch);
                break;

            case CollectibleType.Gem:
                if (Time.time - _lastGemSfxTime < minGemSfxInterval) return;
                _lastGemSfxTime = Time.time;
                if (!string.IsNullOrEmpty(gemSfxKey))
                    AudioManager.instance.PlaySfx(gemSfxKey, sfxVolume, sfxPitch);
                break;
        }
    }
}
