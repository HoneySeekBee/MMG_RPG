using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class ChaseState : IMonsterState
    {
        public void Enter(Monster monster)
        {
            Console.WriteLine($"{monster.Name} → Chase 상태 진입");
        }

        public void Update(Monster monster, float deltaTime)
        {
            if (monster.Target == null || monster.Target.IsDead)
            {
                monster.StateMachine.ChangeState(new PatrolState(), monster);
                return;
            }

            // 이동
            monster.Mover.MoveTo(monster.Target.Position, deltaTime);

            // 공격 사거리 진입 시 공격 상태로 전이
            if (monster.IsInAttackRange())
            {
                monster.StateMachine.ChangeState(new AttackState(), monster);
            }
        }

        public void Exit(Monster monster)
        {
            Console.WriteLine($"{monster.Name} → Chase 상태 종료");
        }
    }
}
