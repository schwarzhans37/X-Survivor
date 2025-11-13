using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GachaRateData", menuName = "Gacha/Gacha Rate Table")]
public class GachaRateData : ScriptableObject
{
    // 등급별 확률 정보를 담을 내부 클래스
    [System.Serializable]
    public class GachaRate
    {
        public string grade;
        [Range(0, 100)]
        public float probability;
        public List<int> petIds;
    }

    public List<GachaRate> rates;
}
