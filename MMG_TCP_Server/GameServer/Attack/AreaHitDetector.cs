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
    internal class AreaHitDetector : IHitDetector
    {
        private float _radius;

        public AreaHitDetector(float radius)
        {
            _radius = radius;
        }

        public List<CharacterStatus> DetectTargets(GameRoom room, CharacterStatus attacker, Vector3 pos, float rotY, WeaponData weapon)
        {
            List<CharacterStatus> targets = room.FindPlayersInArea(pos, _radius);
            targets.RemoveAll(t => t == attacker); // 자해 방지
            return targets;
        }
    }
}
