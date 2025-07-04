using GameServer.Intreface;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Attack
{
    public enum HitDetectorType
    {
        Area,
        Single,
    }
    public class HitDetectorFactory
    {
        public static IHitDetector Create(HitDetectorType type, float param)
        {
            switch (type)
            {
                case HitDetectorType.Area:
                    return new AreaHitDetector(param);
                // case HitDetectorType.Single: return new SingleTargetDetector();
                default:
                    throw new ArgumentException("Unsupported detector type");
            }
        }
        public static IHitDetector Get(AttackType type)
        {
            switch (type)
            {
                case AttackType.MeleeSlash:
                    return new AreaHitDetector(2.0f); // 예시: 2미터 범위 베기

                //case AttackType.MeleeStab:
                //    return new ConeHitDetector(3.0f, 45f); // 예시: 콘 형태 찌르기

                //case AttackType.RangedArrow:
                //    return new ProjectileHitDetector(10.0f); // 원거리 투사체

                case AttackType.MagicExplosion:
                    return new AreaHitDetector(4.0f); // 마법 범위 폭발

                default:
                    return null;
            }
        }
    }
}
