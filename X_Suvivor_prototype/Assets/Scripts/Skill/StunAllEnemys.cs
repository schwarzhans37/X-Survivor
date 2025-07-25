using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunAllEnemys : PetSkillBase
{
    public float stunDuration = 2f;

    public override void Execute()
    {
        Debug.Log($"모든 적을 {stunDuration}초 동안 스턴시킨다.");
    }
}
