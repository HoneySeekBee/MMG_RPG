using GameServer.Data;
using GameServer.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Attack
{
    public class Projectile
    {
        public int OwnerId;
        public Vector3 Position;
        public Vector3 Direction;
        public float Speed;
        public float Radius;
        public float MaxDistance;
        public AttackData AttackData;

        private float _traveled = 0f;

        public bool IsExpired => _traveled >= MaxDistance;

        public void Update(float deltaTime, GameRoom room)
        {
            Vector3 move = Direction * Speed * deltaTime;
            Position += move;
            _traveled += move.Length();

            foreach (var player in room._players.Values)
            {
                if (player.ObjectId == OwnerId)
                    continue;

                Vector3 toTarget = player.Position - Position;

                float dist = toTarget.Length();

                if (dist <= Radius)
                {
                    //  명중
                    room.OnPlayerHit(OwnerId, player.ObjectId, AttackData); // 데미지 처리 등
                    _traveled = MaxDistance; // 즉시 만료 처리
                    break;
                }
            }
        }
    }
}
