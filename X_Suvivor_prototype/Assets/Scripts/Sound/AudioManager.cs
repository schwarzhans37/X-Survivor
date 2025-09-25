using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("# BGM")]
    public AudioClip bgmClip;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmEffect; // optional FX on main camera

    [Header("# SFX")]
    public AudioClip[] sfxClips;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Min(1)] public int channels = 16;
    AudioSource[] sfxPlayers;
    int channelIndex;

    public enum Sfx
    {
        Dead = 0,
        Hit,            // Hit0, Hit1 (연속 인덱스 전제)
        LevelUp = 3,
        Lose,
        Melee = 5,      // Melee0, Melee1 (연속 인덱스 전제)
        Range = 7,
        Select,
        Win,

        // UI & Gacha
        UI_Click,
        Gacha_Start,
        Gacha_Result,
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);   // 중복 방지
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        Init();
    }

    void Init()
    {
        // --- BGM Source ---
        var bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform, false);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // 메인 카메라가 없을 수도 있으므로 널 가드
        bgmEffect = Camera.main ? Camera.main.GetComponent<AudioHighPassFilter>() : null;

        // --- SFX Sources (채널 풀) ---
        var sfxObject = new GameObject("SfxPlayers");
        sfxObject.transform.SetParent(transform, false);
        sfxPlayers = new AudioSource[Mathf.Max(1, channels)];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            var src = sfxObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.bypassListenerEffects = true;
            src.volume = sfxVolume;
            src.spatialBlend = 0f; // 2D
            sfxPlayers[i] = src;
        }
    }

    // ---------------- BGM ----------------

    public void PlayBgm(bool isPlay)
    {
        if (!bgmPlayer) return;
        if (isPlay) bgmPlayer.Play();
        else bgmPlayer.Stop();
    }

    public void PlayBgm(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (!bgmPlayer) return;
        if (clip == null) { bgmPlayer.Stop(); return; }
        bgmPlayer.clip = clip;
        bgmPlayer.volume = Mathf.Clamp01(volume);
        bgmPlayer.loop = loop;
        bgmPlayer.Play();
    }

    public void EffectBgm(bool isPlay)
    {
        if (bgmEffect) bgmEffect.enabled = isPlay;
    }

    // ---------------- SFX ----------------

    // 1) enum 기반 재생
    public void PlaySfx(Sfx sfx)
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int idx = (i + channelIndex) % sfxPlayers.Length;
            var src = sfxPlayers[idx];
            if (src.isPlaying) continue;

            int ran = (sfx == Sfx.Hit || sfx == Sfx.Melee) ? Random.Range(0, 2) : 0;
            int clipIndex = (int)sfx + ran;
            if (clipIndex < 0 || clipIndex >= sfxClips.Length) return;

            channelIndex = idx;
            src.pitch = 1f;
            src.volume = sfxVolume;          // 전역 SFX 볼륨 반영
            src.clip = sfxClips[clipIndex];
            src.Play();
            break;
        }
    }

    // 2) AudioClip 직접 재생 (스킬/임시 SFX용)
    //    예) AudioManager.instance.PlaySfx(clip, 1.1f, 0.9f, 0.35f);
    public void PlaySfx(AudioClip clip, float pitch = 1f, float volumeScale = 1f, float maxDuration = -1f)
    {
        if (clip == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int idx = (i + channelIndex) % sfxPlayers.Length;
            var src = sfxPlayers[idx];
            if (src.isPlaying) continue;

            channelIndex = idx;
            src.pitch = pitch;
            src.volume = Mathf.Clamp01(sfxVolume * volumeScale);
            src.clip = clip;
            src.Play();

            if (maxDuration > 0f)
                StartCoroutine(StopAfter(src, clip, maxDuration));

            break;
        }
    }

    // 안전 가드: 같은 채널이 다른 소리를 재생 중이면 건드리지 않음
    IEnumerator StopAfter(AudioSource src, AudioClip clip, float t)
    {
        yield return new WaitForSeconds(t);
        if (src && src.isPlaying && src.clip == clip) src.Stop();
    }

    // 필요하면 위치 기반(현재는 2D 믹싱이므로 전역 출력; 3D 필요 시 spatialBlend=1 사용)
    public void PlaySfxAt(AudioClip clip, Vector3 worldPos, float pitch = 1f, float volumeScale = 1f, float maxDuration = -1f)
    {
        PlaySfx(clip, pitch, volumeScale, maxDuration); // 2D 게임이면 전역 재생으로 충분
    }

    // 볼륨 변경(옵션)
    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        foreach (var src in sfxPlayers) if (src) src.volume = sfxVolume;
    }

    public void SetBgmVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (bgmPlayer) bgmPlayer.volume = bgmVolume;
    }
}
