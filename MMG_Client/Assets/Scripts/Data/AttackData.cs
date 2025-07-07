using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MMG/Attack")]
public class AttackData : ScriptableObject
{
    public int AttackId;
    public string AttackName;
    public WeaponType WeaponType;
    public AttackType AttackType;

    public float Range;

    public float Angle;
    public float Damage;
    public float Cooldown;
    public float DelayAfter;
    public float CastTime;
}
