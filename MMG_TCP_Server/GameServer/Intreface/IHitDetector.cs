using GameServer.Domain;
using GameServer.GameRoomFolder;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Intreface
{
    public interface IHitDetector
    {
        List<CharacterStatus> DetectTargets(GameRoom room, CharacterStatus attacker, Vector3 pos, float rotY, WeaponData weapon);
    }
}
