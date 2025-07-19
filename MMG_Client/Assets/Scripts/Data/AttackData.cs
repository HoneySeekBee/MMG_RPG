using AttackPacket;
using GamePacket;
using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackData : ScriptableObject
{
    public int AttackId;
    public string AttackName;
    public OwnerType OwnerType;
    public WeaponType WeaponType;
    public AttackType AttackType;

    public int SkillLevel;
    public int SlotIndex;

    public int Appearance; // 몬스터용

    public float Range;
    public float Angle;
    public float Damage;
    public float Cooldown;
    public float DelayAfter;
    public float CastTime;

}
