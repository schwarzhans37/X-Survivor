using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemsCountUI : MonoBehaviour
{
    public Text gemsText;

    void LateUpdate()
    {
        gemsText.text = $"{GameManager.instance.acquiredGems}";
    }
}
