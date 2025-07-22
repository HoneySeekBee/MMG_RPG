using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
    private RemotePlayer remotePlayer;
    public void GetRemotePlayer(RemotePlayer _remotePlayer)
    {
        remotePlayer = _remotePlayer;
    }
    public override void UpdateMoveAnimation(float speed, float dir)
    {
        if (remotePlayer == null && remotePlayer.isDead)
            return;

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
