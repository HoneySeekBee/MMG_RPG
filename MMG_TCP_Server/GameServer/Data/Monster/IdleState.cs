using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class IdleState : IMonsterState
    {
        private float _timer;

        public void Enter(MonsterObject monster)
        {
            _timer = 2f; // 대기 시간
            Console.WriteLine($"[FSM] {monster.objectInfo.Name} -> Idle");
        }

        public void Update(MonsterObject monster, float deltaTime)
        {
            _timer -= deltaTime;

            if (_timer <= 0f)
            {
                monster.StateMachine.ChangeState(new PatrolState(), monster);
            }
        }

        public void Exit(MonsterObject monster)
        {
            Console.WriteLine($"[FSM] {monster.objectInfo.Name} exiting Idle");
        }
    }
}
