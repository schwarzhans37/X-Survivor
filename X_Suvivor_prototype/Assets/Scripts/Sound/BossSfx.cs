using UnityEngine;

public class BossSfx : MonoBehaviour
{
    [Header("Clips (직접 재생)")]
    public AudioClip introClip;   // 선택: 보스 등장/컷신 등에서 쓰고 싶으면 연결
    public AudioClip meleeClip;   // 근접 공격 효과음

    [Header("Keys (SoundData 재생)")]
    public string fireKey = "FireBoss";
    public string lightningKey = "LightningBoss";

    [Header("믹싱")]
    [Range(0f, 3f)] public float volume = 1f;      // 개별 볼륨 스케일
    [Range(0.1f, 3f)] public float pitch = 1f;     // 개별 피치

    [Header("개발 편의")]
    public bool logNoopCalls = false;

    // ===== Animation Event에서 호출 =====
    public void PlayIntro() => PlayClip(introClip);
    public void PlayIntroOnce()
    {
        if (_introPlayed) return;
        _introPlayed = true;
        PlayIntro();
    }

    public void PlayMelee() => PlayClip(meleeClip);

    // 캐스트 시점 재생: Fire / Lightning만 유지
    public void PlayFire() => PlayKey(fireKey);
    public void PlayLightning() => PlayKey(lightningKey);

    // Magic은 **캐스트 시** 소리 X → 애니 이벤트가 남아있다면 NO-OP 처리
    public void PlayMagic()
    {
        if (logNoopCalls) Debug.Log("[BossSfx] PlayMagic NO-OP (hit에서 재생)");
    }

    // ---------------- 내부 공용 함수 ----------------
    void PlayClip(AudioClip clip)
    {
        if (!clip) return;

        var go = new GameObject("SFX_BossOneShot");
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;   // BossSfx 인스펙터 슬라이더
        src.pitch = pitch;
        src.spatialBlend = 0f; // 2D
        src.Play();

        // 피치가 걸린 길이만큼 살려두고 파괴
        Destroy(go, clip.length / Mathf.Max(0.01f, src.pitch));
    }


    void PlayKey(string key)
    {
        if (string.IsNullOrEmpty(key) || AudioManager.instance == null) return;
        AudioManager.instance.PlaySfx(key, volume, pitch);
    }

    bool _introPlayed;
    public void ResetIntroFlag() => _introPlayed = false;
}
