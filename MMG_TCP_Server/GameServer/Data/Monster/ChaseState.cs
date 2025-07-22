using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class ChaseState : IMonsterState
    {
        public void Enter(MonsterObject monster)
        {
            Console.WriteLine($"{monster.objectInfo.Name} → Chase 상태 진입");
        }

        public void Update(MonsterObject monster, float deltaTime)
        {
            if (monster.Target == null)
            {
                monster.StateMachine.ChangeState(new PatrolState(), monster);
                return;
            }

            if (monster.IsDead)
            {
                monster.StateMachine.ChangeState(new DeadState(), monster);
                return;
            }
            // 이동
            monster.Mover.MoveTo(monster.Target.Position, deltaTime);

            // 공격 사거리 진입 시 공격 상태로 전이
            if (monster.IsInAttackRange())
            {
                monster.StateMachine.ChangeState(new AttackState(), monster);
            }
            if(monster.IsInChaseRange() == false)
            {
                monster.ClearTarget();
                monster.StateMachine.ChangeState(new PatrolState(), monster);
            }
        }

        public void Exit(MonsterObject monster)
        {
            Console.WriteLine($"{monster.objectInfo.Name} → Chase 상태 종료");
        }
    }
}
