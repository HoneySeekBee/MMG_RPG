using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class MonsterMover
    {
        private readonly MonsterObject _monster;

        public MonsterMover(MonsterObject monster)
        {
            _monster = monster;
        }
        public void MoveTo(Vector3 destination, float deltaTime)
        {
            Vector3 toTarget = destination - _monster.Position;
            float distance = toTarget.Length();

            if (distance < 0.001f)
                return;

            Vector3 dir = toTarget / distance;
            Vector3 move = dir * _monster.Status.MoveSpeed * deltaTime;

            // 목적지 초과 이동 방지
            if (move.LengthSquared() > distance * distance)
                move = toTarget;

            _monster.AddPosition(move);

            // 회전값 계산도 여기서 처리할 수 있음 (선택)
            _monster.SetDirectionY(MathF.Atan2(dir.X, dir.Z) * (180f / MathF.PI));
        }


        public void SetPosition(Vector3 position)
        {
            _monster.SetPosition(position);
        }
        public void LookAt(Vector3 targetPos)
        {
            Vector3 dir = targetPos - _monster.Position;
            if (dir.LengthSquared() < 0.0001f)
                return;

            float angle = MathF.Atan2(dir.X, dir.Z) * (180f / MathF.PI);
            _monster.SetDirectionY(angle);
        }
    }
}
