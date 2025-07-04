using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType { Exp, Level, Kill, Time, Health }
    public InfoType type;

    Text myText;
    Slider mySlider;

    void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    void LateUpdate()
    {
        Player player = GameManager.instance.player;

        switch (type)
        {
            case InfoType.Exp:
                float currentExp = GameManager.instance.nowExp;
                float maxExp = GameManager.instance.needExp[Mathf.Min(GameManager.instance.level, GameManager.instance.needExp.Length - 1)];
                mySlider.value = currentExp / maxExp;
                break;
            case InfoType.Level:
                myText.text = string.Format("Lv.{0:F0}", GameManager.instance.level);
                break;
            case InfoType.Kill:
                myText.text = string.Format("{0:F0}", GameManager.instance.killCount);
                break;
            case InfoType.Time:
                float remainTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
                int min = Mathf.FloorToInt(remainTime / 60);
                int sec = Mathf.FloorToInt(remainTime % 60);
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                break;
            case InfoType.Health:
                float currentHp = player.health;
                float maxHp = player.maxHealth;
                mySlider.value = currentHp / maxHp;
                break;
        }
    }
}
