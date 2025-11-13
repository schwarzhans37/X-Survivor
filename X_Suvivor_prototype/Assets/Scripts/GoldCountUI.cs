using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldCountUI : MonoBehaviour
{
    public Text goldText;

    void LateUpdate()
    {
        goldText.text = $"{GameManager.instance.acquiredGold}";
    }
}
