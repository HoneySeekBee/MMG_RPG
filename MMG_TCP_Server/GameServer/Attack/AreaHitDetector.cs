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
        public List<GameObject> DetectTargets(GameRoom room, GameObject attacker, Vector3 pos, float rotY, Skill attackData)
        {
            List<GameObject> result = new List<GameObject>();

            // 공격 방향 벡터 (y축 회전 각도 → 라디안 → 벡터)
            float rad = rotY * (float)Math.PI / 180f;
            float forwardX = (float)Math.Sin(rad);
            float forwardZ = (float)Math.Cos(rad);

            float radiusSqr = attackData.Range * attackData.Range;
            float cosThreshold = (float)Math.Cos(attackData.Angle * 0.5f * Math.PI / 180f); // 반각 기준

            float innerRadius = 0.3f; // 가까이 붙은 적 허용 반경 (튜닝 가능)
            float innerRadiusSqr = innerRadius * innerRadius;

            List<GameObject> targets = new List<GameObject>();

            if(attacker.Type == ObjectType.Monster) // 몬스터의 공격은 플레이어가 맞는다. 
            {
                foreach (GameObject target in room._players.Values)
                {
                    targets.Add(target);
                }
            }
            else // 플레이어의 공격은 몬스터가 맞는다. 
            {
                foreach (GameObject target in room._monsters.Values)
                {
                    targets.Add(target);
                }
            }

            foreach (var player in targets)
            {
                float dx = player.Position.X - pos.X;
                float dz = player.Position.Z - pos.Z;
                float dy = player.Position.Y - pos.Y;

                float sqrDist = dx * dx + dy * dy + dz * dz;

                Console.WriteLine($"[AreaHitDetector] (1) {sqrDist} {radiusSqr}");

                if (sqrDist > radiusSqr)
                    continue;

                Console.WriteLine($"[AreaHitDetector] (2) {sqrDist} {innerRadiusSqr}");

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


                Console.WriteLine($"[AreaHitDetector] (3) {dot} {cosThreshold}");

                if (dot >= cosThreshold)
                    result.Add(player);
            }

            return result;
        }

    }
}
