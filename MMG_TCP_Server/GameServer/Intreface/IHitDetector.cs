using GamePacket;
using GameServer.Data;
using GameServer.Game.Object;
using GameServer.Game.Room;
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
        List<CharacterObject> DetectTargets(GameRoom room, CharacterObject attacker, Vector3 pos, float rotY, Skill attackData);
    }
}
