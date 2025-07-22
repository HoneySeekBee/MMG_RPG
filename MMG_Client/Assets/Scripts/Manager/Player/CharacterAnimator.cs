using DevionGames.UIWidgets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterAnimator : MonoBehaviour
{
    protected Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public abstract void UpdateMoveAnimation(float speed, float dir);
    public abstract void PlayAttack(bool isLeft);
    public virtual void GetDamaged()
    {
        animator.Play("Damage", 0);
    }
    public virtual void DeadAnimation()
    {
        animator.Play("Death", 0);
    }
    public virtual void IdleAnimation()
    {
        animator.Play("Idle", 0);

    }
}
