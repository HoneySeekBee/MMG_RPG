using GameServer.Data;
using GameServer.Intreface;
using Packet;
using System.Numerics;
using GameServer.Game.Room;
using GameServer.Game.Object;
using GamePacket;
using AttackPacket;
using System.Diagnostics;

namespace GameServer.Attack
{
    public class BattleSystem
    {
        private GameRoom _room;
        public BattleSystem(GameRoom room)
        {
            _room = room;
        }
        public void HandleAttack(GameObject attacker, Vector3 pos, float rotY, Skill attackData)
        {
            Console.WriteLine($"[BattleSystem] AttackerId : {attacker.objectInfo.Id}");
            bool isProjectile = attackData.AttackType == AttackType.Arrow; 
            if (isProjectile)
            {
                Console.WriteLine("[BattleSystem] 원거리 공격");
                _room.LaunchProjectile(attacker, pos, rotY, attackData);
            }
            else
            {
                IHitDetector detector = HitDetectorFactory.Get(attackData.AttackType);
                var targets = detector.DetectTargets(_room, attacker, pos, rotY, attackData);
                Console.WriteLine("[BattleSystem]  [HandleAttack] " + attacker.Type);
                foreach (var target in targets)
                {
                    target.OnDamaged(attacker, attackData.Damage);
                    S_DamageBroadcast DamageBroadcast = new S_DamageBroadcast();
                    DamageBroadcast.Damage = new DamageInfo()
                    {
                        TargetId = target.objectInfo.Id,
                        Damage = attackData.Damage,
                        AttackerId = attacker.ObjectId,
                        IsMonster = attacker.Type == ObjectType.Monster? true :false,
                    };
                    _room.BroadcastDamage(DamageBroadcast);
                }
            }
        }
    }
}
