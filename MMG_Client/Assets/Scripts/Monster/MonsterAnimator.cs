using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimator : CharacterAnimator
{
    public override void UpdateMoveAnimation(float speed, float dir)
    {
        animator.SetFloat("speed", speed);
        animator.SetFloat("dir", dir);
        animator.SetBool("isMoving", speed > 0.01f);
    }
    public override void PlayAttack(bool isLeft)
    {
        string triggerName = isLeft ? "isNormalAttack" : "isCriticalAttack";
        animator.SetTrigger(triggerName);
    }

}
