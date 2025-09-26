using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class UIButtonSfx : MonoBehaviour
{
    [SerializeField] public AudioManager.Sfx onClick = AudioManager.Sfx.UI_Click;

    void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(onClick);
    }
}
