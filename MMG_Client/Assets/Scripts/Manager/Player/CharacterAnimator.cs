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
}
