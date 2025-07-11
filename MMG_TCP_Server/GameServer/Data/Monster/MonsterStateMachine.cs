using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class MonsterStateMachine
    {
        private IMonsterState _currentState;

        public void ChangeState(IMonsterState newState, MonsterObject monster)
        {
            _currentState?.Exit(monster);
            _currentState = newState;
            _currentState?.Enter(monster);
        }

        public void Update(MonsterObject monster, float deltaTime)
        {
            _currentState?.Update(monster, deltaTime);
        }
    }
}
