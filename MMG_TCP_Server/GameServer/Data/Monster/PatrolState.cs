using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class PatrolState : IMonsterState
    {
        public void Enter(Monster monster)
        {
            Console.WriteLine($"{monster.Name} → Patrol 상태 진입");
        }

        public void Update(Monster monster, float deltaTime)
        {
            // 1. 목표 지점까지 거리 확인
            Vector3 targetPos = monster.CurrentPatrolPoint;
            float distance = Vector3.Distance(monster.Position, targetPos);

            // 2. 목표 지점에 거의 도달했으면 다음 웨이포인트로
            if (distance < 0.1f)
            {
                monster.MoveToNextPatrolPoint();
                targetPos = monster.CurrentPatrolPoint;
            }

            // 3. 이동 명령
            monster.Mover.MoveTo(targetPos, deltaTime);

            // 4. 시야 내 플레이어 감지
            var visiblePlayer = monster.Sensor.FindVisiblePlayer();
            if (visiblePlayer != null)
            {
                monster.SetTarget(visiblePlayer);
                monster.StateMachine.ChangeState(new ChaseState(), monster);
            }
        }

        public void Exit(Monster monster)
        {
            Console.WriteLine($"{monster.Name} → Patrol 상태 종료");
        }
    }
}
