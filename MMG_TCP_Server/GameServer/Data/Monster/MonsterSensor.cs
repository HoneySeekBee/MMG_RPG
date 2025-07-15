using GameServer.Game.Object;
using GameServer.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class MonsterSensor
    {
        private readonly MonsterObject _monster;
        private readonly GameRoom _room;
        private float DetectRange = 5f;

        public MonsterSensor(MonsterObject monster, GameRoom room)
        {
            _monster = monster;
            _room = room;
            DetectRange = monster.Status.MonsterData.ChaseRange;
        }


        public CharacterObject FindVisiblePlayer()
        {
            foreach (var player in _room._players.Values)
            {
                // 1. 본인과 동일 ID는 제외
                if (player.ObjectId == _monster.objectInfo.Id)
                    continue;


                // 2. 사망한 플레이어 무시
                if (player.IsDead)
                    continue;

                // 3. 거리 기반 감지 (TODO: 시야각 등 확장 가능)
                float dist = Vector3.Distance(player.Position, _monster.Position);
                if (dist <= DetectRange)
                {
                    return player;
                }
            }

            return null; // 아무도 감지 못함
        }
    }
}
