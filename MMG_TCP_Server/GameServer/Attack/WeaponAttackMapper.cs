using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Attack
{
    public static class WeaponAttackMapper
    {
        private static readonly Dictionary<WeaponType, AttackType> Map = new()
    {
        { WeaponType.Sword,       AttackType.MeleeSlash },
        { WeaponType.GreatSword,  AttackType.MeleeCleave },
        { WeaponType.Spear,       AttackType.MeleeStab },
        { WeaponType.Hammer,      AttackType.MeleeCleave },
        { WeaponType.Axe,         AttackType.MeleeSlash },
        { WeaponType.Fist,        AttackType.MeleeFist },
        { WeaponType.Bow,         AttackType.RangedArrow },
        { WeaponType.Staff,       AttackType.RangedMagicBolt },
        { WeaponType.Tome,        AttackType.MagicExplosion },
        { WeaponType.Instrument,  AttackType.MagicBuff },
        // 기타 추가 가능
    };

        public static AttackType GetAttackType(WeaponType weaponType)
            => Map.TryGetValue(weaponType, out var atkType) ? atkType : AttackType.NotAttack;
    }
}
