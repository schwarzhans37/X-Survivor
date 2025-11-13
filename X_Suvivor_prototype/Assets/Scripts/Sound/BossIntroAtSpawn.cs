using UnityEngine;

[RequireComponent(typeof(BossSfx))]
public class BossIntroAtSpawn : MonoBehaviour
{
    BossSfx sfx;

    void Awake() => sfx = GetComponent<BossSfx>();

    void OnEnable()
    {
        // 프리팹이 씬/풀에서 '활성화' 될 때마다 호출됨
        sfx?.PlayIntroOnce();
    }

    // (선택) 풀 시스템에서 비활성화 직전에 호출하고 싶다면 이 메서드를 끌어다 써
    public void ResetIntro() => sfx?.ResetIntroFlag();
}
