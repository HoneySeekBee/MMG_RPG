using GameServer.Data;
using GameServer.Domain;
using GameServer.GameRoomFolder;
using GameServer.Intreface;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Attack
{
    public class BattleSystem
    {
        private GameRoom _room;
        public BattleSystem(GameRoom room)
        {
            _room = room;
        }
        public void HandleAttack(CharacterStatus attacker, Vector3 pos, float rotY, AttackData attackData)
        {
            Console.WriteLine($"[BattleSystem] AttackerId : {attacker.Id}");
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

                foreach (var target in targets)
                {
                    target.OnDamaged(attacker, attackData.Damage);
                    _room.BroadcastDamage(new S_DamageBroadcast
                    {
                        TargetId = target.Id,
                        Damage = attackData.Damage,
                        AttackerId = attacker.Id
                    });
                }
            }
        }
    }
}
