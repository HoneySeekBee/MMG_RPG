using AttackPacket;
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
                    return new AreaHitDetector();
                // case HitDetectorType.Single: return new SingleTargetDetector();
                default:
                    throw new ArgumentException("Unsupported detector type");
            }
        }
        public static IHitDetector Get(AttackType type)
        {
            return type switch
            {
                AttackType.Punch or AttackType.Slash => new AreaHitDetector(),
                _ => new AreaHitDetector(), // 기본은 근접
            };
        }
    }
}
