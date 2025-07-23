using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PetSkillBase : MonoBehaviour
{
    // 스킬의 기본 정보를 담을 수 있음
    public float cooldown;

    // 이 메소드를 각 스킬을 각자의 방식대로 구현(override)해야 한다.
    public abstract void Execute();
}
