using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSoundLoader : MonoBehaviour
{
    [Tooltip("이 씬에서 사용할 사운드 데이터를 여기에 할당해주세요.")]
    public SoundData sceneSoundData;

    void Start()
    {
        // AudioManager 인스턴스가 존재하는지 확인합니다.
        if (AudioManager.instance != null)
        {
            // AudioManager에게 이 씬의 SoundData를 로드하고 BGM 재생을 요청합니다.
            AudioManager.instance.LoadAndPlaySceneSounds(sceneSoundData);
        }
        else
        {
            Debug.LogError("AudioManager 인스턴스를 찾을 수 없습니다! 씬에 AudioManager가 있는지 확인해주세요.");
        }
    }
}
