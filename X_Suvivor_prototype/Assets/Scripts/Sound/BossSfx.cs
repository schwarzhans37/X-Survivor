using UnityEngine;

public class BossSfx : MonoBehaviour
{
    [Header("Clips (직접 재생)")]
    public AudioClip introClip;   // 선택: 보스 등장/컷신 등에서 쓰고 싶으면 연결
    public AudioClip meleeClip;   // 근접 공격 효과음

    [Header("Keys (SoundData 재생)")]
    public string fireKey = "FireBoss";
    public string lightningKey = "LightningBoss";
    public string magicKey = "MagicBoss";

    [Header("믹싱")]
    [Range(0f, 3f)] public float volume = 1f;      // 개별 볼륨 스케일
    [Range(0.1f, 3f)] public float pitch = 1f;     // 개별 피치

    // ===== Animation Event에서 호출 =====
    public void PlayIntro() { PlayClip(introClip); }
    public void PlayMelee() { PlayClip(meleeClip); }

    public void PlayFire() { PlayKey(fireKey); }
    public void PlayLightning() { PlayKey(lightningKey); }
    public void PlayMagic() { PlayKey(magicKey); }

    // ---------------- 내부 공용 함수 ----------------
    void PlayClip(AudioClip clip)
    {
        if (!clip || AudioManager.instance == null) return;
        AudioManager.instance.PlaySfx(clip); // AudioClip 직접 재생
    }

    void PlayKey(string key)
    {
        if (string.IsNullOrEmpty(key) || AudioManager.instance == null) return;
        // 너의 AudioManager 시그니처: PlaySfx(string name, float volumeScale, float pitch)
        AudioManager.instance.PlaySfx(key, volume, pitch);
    }

    bool _introPlayed;                   // 인트로 1회 재생 가드

    public void PlayIntroOnce()          // 인트로를 1번만 재생
    {
        if (_introPlayed) return;
        _introPlayed = true;
        PlayIntro();
    }

    public void ResetIntroFlag() => _introPlayed = false;   // (선택) 풀 재사용 시 초기화
}
