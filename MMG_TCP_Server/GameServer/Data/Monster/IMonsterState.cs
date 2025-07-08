using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public interface IMonsterState
    {
        void Enter(Monster monster);
        void Update(Monster monster, float deltaTime);
        void Exit(Monster monster);
    }
}
