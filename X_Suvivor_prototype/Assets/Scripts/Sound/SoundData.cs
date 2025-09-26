using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    // 사운드 데이터 관리용 내부 클래스
    [System.Serializable]
    public class Sound
    {
        public string name;     // 사운드를 찾기 위한 키(Key)
        public AudioClip clip;  // 실제 오디오 클립
    }

    public List<Sound> sounds = new List<Sound>();
}
