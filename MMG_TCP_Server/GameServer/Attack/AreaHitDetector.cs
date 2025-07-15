using GameServer.Data;
using GameServer.Intreface;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using GameServer.Game.Room;
using GameServer.Game.Object;
using GamePacket;

namespace GameServer.Attack
{
    internal class AreaHitDetector : IHitDetector
    {
        public List<CharacterObject> DetectTargets(GameRoom room, CharacterObject attacker, Vector3 pos, float rotY, Skill attackData)
        {
            Console.WriteLine("근거리 공격");
            List<CharacterObject> result = new List<CharacterObject>();

            // 공격 방향 벡터 (y축 회전 각도 → 라디안 → 벡터)
            float rad = rotY * (float)Math.PI / 180f;
            float forwardX = (float)Math.Sin(rad);
            float forwardZ = (float)Math.Cos(rad);

            float radiusSqr = attackData.Range * attackData.Range;
            float cosThreshold = (float)Math.Cos(attackData.Angle * 0.5f * Math.PI / 180f); // 반각 기준

            float innerRadius = 0.3f; // 가까이 붙은 적 허용 반경 (튜닝 가능)
            float innerRadiusSqr = innerRadius * innerRadius;

            foreach (var player in room._players.Values)
            {
                if (player.ObjectId == attacker.ObjectId)
                    continue;

                float dx = player.Position.X - pos.X;
                float dz = player.Position.Z - pos.Z;
                float dy = player.Position.Y - pos.Y;

                float sqrDist = dx * dx + dy * dy + dz * dz;
                if (sqrDist > radiusSqr)
                    continue;

                if (sqrDist <= innerRadiusSqr)
                {
                    result.Add(player);
                    continue;
                }

                // 정규화된 방향 벡터 (toTarget)
                float length = (float)Math.Sqrt(sqrDist);
                float dirX = dx / length;
                float dirZ = dz / length;

                // dot(forward, toTarget)
                float dot = forwardX * dirX + forwardZ * dirZ;

                if (dot >= cosThreshold)
                    result.Add(player);
            }

            return result;
        }

    }
}
