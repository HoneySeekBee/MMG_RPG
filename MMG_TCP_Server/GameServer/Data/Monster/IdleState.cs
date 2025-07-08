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

        public void Enter(Monster monster)
        {
            _timer = 2f; // 대기 시간
            Console.WriteLine($"[FSM] {monster.Name} -> Idle");
        }

        public void Update(Monster monster, float deltaTime)
        {
            _timer -= deltaTime;

            if (_timer <= 0f)
            {
                monster.StateMachine.ChangeState(new PatrolState(), monster);
            }
        }

        public void Exit(Monster monster)
        {
            Console.WriteLine($"[FSM] {monster.Name} exiting Idle");
        }
    }
}
