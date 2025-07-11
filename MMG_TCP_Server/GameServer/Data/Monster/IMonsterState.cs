using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public interface IMonsterState
    {
        void Enter(MonsterObject monster);
        void Update(MonsterObject monster, float deltaTime);
        void Exit(MonsterObject monster);
    }
}
