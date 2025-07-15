using AttackPacket;
using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MMG
{
    [System.Serializable]
    public class AttackDataListWrapper
    {
        public List<AttackDataDTO> attackList;

        public AttackDataListWrapper(List<AttackDataDTO> list)
        {
            attackList = list;
        }
        [System.Serializable]
        public class AttackDataDTO
        {
            public int AttackId;
            public string AttackName;
            public WeaponType WeaponType;
            public AttackType AttackType;

            public float Range;
            public float Angale;
            public float Damage;
            public float Cooldown;
            public float DelayAfter;
            public float CastTime;

            public AttackDataDTO(AttackData data)
            {
                AttackId = data.AttackId;
                AttackName = data.AttackName;
                WeaponType = data.WeaponType;
                AttackType = data.AttackType;
                Range = data.Range;
                Angale = data.Angle;
                Damage = data.Damage;
                Cooldown = data.Cooldown;
                DelayAfter = data.DelayAfter;
                CastTime = data.CastTime;
            }
        }
    }
}