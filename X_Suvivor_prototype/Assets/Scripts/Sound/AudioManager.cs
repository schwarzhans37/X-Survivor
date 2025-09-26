using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("# 컴포넌트")]
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmEffect;
    AudioSource[] sfxPlayers;

    [Header("# 볼륨설정")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("# 효과음 설정")]
    [Min(1)] public int channels = 16;
    int channelIndex;

    // 현재 로드된 사운드 데이터와 빠른 조회를 위한 딕셔너리
    private SoundData currentSoundData;
    private Dictionary<string, AudioClip> soundBank;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 오디오 플레이어들 초기 생성
        Init();
    }

    void Init()
    {
        // --- BGM 데이터 ---
        var bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform, false);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;

        // 메인 카메라가 없을 경우를 대비한 Null 방지
        bgmEffect = Camera.main ? Camera.main.GetComponent<AudioHighPassFilter>() : null;

        // --- SFX 데이터 (채널 풀) ---
        var sfxObejct = new GameObject("SfxPlayers");
        sfxObejct.transform.SetParent(transform, false);
        sfxPlayers = new AudioSource[Mathf.Max(1, channels)];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            var src = sfxObejct.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.bypassListenerEffects = true;
            sfxPlayers[i] = src;
        }
    }

    // 새로운 SoundData 에셋을 로드하고 해당 씬의 BGM을 재생
    public void LoadAndPlaySceneSounds(SoundData soundData)
    {
        if (soundData == null)
        {
            Debug.LogWarning("로드할 SoundData가 없습니다.");
            return;
        }

        currentSoundData = soundData;
        soundBank = new Dictionary<string, AudioClip>();
        foreach (var sound in currentSoundData.sounds)
        {
            if (!soundBank.ContainsKey(sound.name))
            {
                soundBank.Add(sound.name, sound.clip);
            }
        }

        // BGM 자동 재생 (SoundData에 "BGM"이라는 이름의 클립이 있다면)
        if (soundBank.ContainsKey("BGM"))
        {
            AudioClip newBgmClip = soundBank["BGM"];
            // BGM이 바뀌었을 경우에만 새로 재생
            if (newBgmClip != null && bgmPlayer.clip != newBgmClip)
            {
                bgmPlayer.clip = newBgmClip;
                bgmPlayer.volume = masterVolume * bgmVolume;
                bgmPlayer.Play();
            }
            // 새 씬의 BGM이 없다면 기존 BGM 정지
            else if (newBgmClip == null)
            {
                bgmPlayer.Stop();
            }
        }
        else
        {
            bgmPlayer.Stop(); // "BGM" 키가 없으면 일단 정지
        }
    }

    public void StopBgm()
    {
        if (bgmPlayer != null) bgmPlayer.Stop();
    }

    public void EffectBgm(bool isPlay)
    {
        if (bgmEffect != null) bgmEffect.enabled = isPlay;
    }

    public void PlaySfx(string sfxName)
    {
        if (soundBank == null) return;

        // "Hit", "Melee" 같은 이름을 받으면 "Hit0", "Hit1" 등으로 랜덤 재생 처리
        string clipName = sfxName;
        if (sfxName == "Hit" || sfxName == "Melee")
        {
            clipName = sfxName + Random.Range(0, 2);
        }

        if (!soundBank.ContainsKey(clipName))
        {
            Debug.LogWarning("SoundBank에서 Sfx를 찾을 수 없습니다: " + clipName);
            return;
        }

        AudioClip clip = soundBank[clipName];
        if (clip == null) return;

        // 비어있는 채널을 순환하며 찾아 재생
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int idx = (i + channelIndex) % sfxPlayers.Length;
            var src = sfxPlayers[idx];
            if (src.isPlaying) continue;

            channelIndex = idx;
            src.clip = clip;
            src.volume = masterVolume * sfxVolume;
            src.pitch = 1f;
            src.Play();
            break;
        }
    }

    // AudioClip을 직접 받아 재생하는 버전 (임시 효과음 등에 유용)
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int idx = (i + channelIndex) % sfxPlayers.Length;
            var src = sfxPlayers[idx];
            if (src.isPlaying) continue;

            channelIndex = idx;
            src.clip = clip;
            src.volume = masterVolume * sfxVolume;
            src.pitch = 1f;
            src.Play();
            break;
        }
    }
}