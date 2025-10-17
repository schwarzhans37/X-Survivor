using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitBox : MonoBehaviour
{
    public float damage;
    public int penetration; // 한 번의 휘두르기로 여러 적을 맞출 수 있는 횟수

    private int hitCount; // 이번 공격에서 실제로 맞춘 적의 수

    // 공격이 시작될 때(오브젝트가 활성화될 때) 호출
    void OnEnable()
    {
        // 이전에 맞춘 횟수를 초기화
        hitCount = 0;
    }

    public void Init(float dmg, int pene)
    {
        damage = dmg;
        penetration = pene;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 관통 횟수가 -100이면 무한 관통(원래 로직 유지)
        if (penetration != -100)
        {
            // 이미 최대 관통 횟수만큼 맞췄다면 더 이상 피해를 주지 않음
            if (hitCount >= penetration)
            {
                return;
            }
        }
        
        // Enemy 태그가 아니면 무시
        if (!collision.CompareTag("Enemy"))
            return;

        hitCount++; // 적을 맞췄으므로 카운트 증가
        // Enemy.cs가 데미지 처리를 하므로, 여기서는 특별히 할 일이 없습니다.
    }
}
