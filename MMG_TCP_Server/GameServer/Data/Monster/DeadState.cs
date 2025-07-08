using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class DeadState : IMonsterState
    {
        public void Enter(Monster monster)
        {
            Console.WriteLine($"{monster.Name} → Dead 상태 진입");
            monster.OnDeath();
        }

        public void Update(Monster monster, float deltaTime)
        {
            // 아무 것도 하지 않음
        }

        public void Exit(Monster monster)
        {
            // 일반적으로 Dead 상태에서 Exit 없음
        }
    }
}
