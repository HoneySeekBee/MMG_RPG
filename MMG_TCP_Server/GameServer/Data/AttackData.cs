using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class AttackData
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
}
