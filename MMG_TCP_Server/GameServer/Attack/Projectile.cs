using GameServer.Data;
using GameServer.GameRoomFolder;
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
            Console.WriteLine($"원거리 Update {OwnerId}, {deltaTime}, {Speed}, {Direction.X}, {Direction.Y}, {Direction.Z}");
            foreach (var player in room._players.Values)
            {
                if (player.Status.Id == OwnerId)
                    continue;

                Vector3 toTarget = player.Position - Position;

                float dist = toTarget.Length();
                Console.WriteLine($"[투사체 체크] Target:{player.Status.Id}, Distance: {dist}, Threshold: {Radius}");

                if (dist <= Radius)
                {
                    //  명중
                    Console.WriteLine($"[Projectile] {OwnerId} hit {player.Status.Id}");
                    room.OnPlayerHit(OwnerId, player.Status.Id, AttackData); // 데미지 처리 등
                    _traveled = MaxDistance; // 즉시 만료 처리
                    break;
                }
            }
        }
    }
}
