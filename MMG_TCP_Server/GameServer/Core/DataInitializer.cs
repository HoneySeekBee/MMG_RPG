using GameServer.Data.Monster;
using GameServer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public static class DataInitializer
    {
        public static async Task InitAsync()
        {
            Console.WriteLine("데이터 로드 시작..");
            await LoadAll();
            Console.WriteLine("데이터 로드 완료..");
        }

        private static async Task LoadAll()
        {
            await SkillDataManager.LoadAttackData();
            await MonsterDataManager.LoadData();
        }
    }
}
