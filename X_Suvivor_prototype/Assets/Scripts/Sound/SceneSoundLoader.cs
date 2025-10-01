using UnityEngine;

public class SceneSoundLoader : MonoBehaviour
{
    [Header("이 씬에서 사용할 SoundData(ScriptableObject)")]
    public SoundData sceneSoundData;

    void Start()
    {
        if (AudioManager.instance != null && sceneSoundData != null)
            AudioManager.instance.LoadAndPlaySceneSounds(sceneSoundData);
        else
            Debug.LogWarning("[SceneSoundLoader] AudioManager or SceneSoundData is null.");
    }
}
