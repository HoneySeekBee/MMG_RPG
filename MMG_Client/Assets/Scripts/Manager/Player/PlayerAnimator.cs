using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void UpdateMoveAnimation(float speed, float dir)
    {
        animator.SetFloat("speed", speed);
        animator.SetFloat("dir", dir);
        animator.SetBool("isMoving", speed > 0.05f);
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

}
