using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class AttackState : IMonsterState
    {
        private float _attackCooldown;

        public void Enter(MonsterObject monster)
        {
            Console.WriteLine($"{monster.objectInfo.Name} → Attack 상태 진입");
            _attackCooldown = 0f;
        }

        public void Update(MonsterObject monster, float deltaTime)
        {
            if (monster.Target == null || monster.Target.IsDead)
            {
                monster.StateMachine.ChangeState(new PatrolState(), monster);
                return;
            }

            _attackCooldown -= deltaTime;
            if (_attackCooldown <= 0f)
            {
                monster.AttackTarget();
                _attackCooldown = 1;
                Console.WriteLine("[AttackState] _attackCooldown은 Json 파싱 데이터를 통해 찾으세요");
            }

            if (!monster.IsInAttackRange())
            {
                monster.StateMachine.ChangeState(new ChaseState(), monster);
            }
        }

        public void Exit(MonsterObject monster)
        {
            Console.WriteLine($"{monster.objectInfo.Name} → Attack 상태 종료");
        }
    }
}
