using MonsterPacket;
using Newtonsoft.Json;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class MonsterDataManager
    {
        private static Dictionary<int, MonsterStatus> _monsterDataDict = new();

        public static MonsterStatus Get(int monsterId)
        {
            return _monsterDataDict.TryGetValue(monsterId, out var data) ? data : null;
        }
        public static void LoadData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string rootDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..")); // GameServer 프로젝트 루트
            try
            {
                string fullPath = Path.Combine(rootDir, "Resources", "MonsterData", "monster_data.json");

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"[MonsterDataLoader] 파일 없음: {fullPath}");
                    return;
                }

                string json = File.ReadAllText(fullPath);
                var wrapper = JsonConvert.DeserializeObject<MonsterDataListWrapper>(json);

                if (wrapper == null || wrapper.monsterList == null)
                {
                    Console.WriteLine("[MonsterDataLoader] JSON 구조가 올바르지 않음");
                    return;
                }

                _monsterDataDict = wrapper.monsterList.ToDictionary(x => x.MonsterId, x => x);
                Console.WriteLine($"[MonsterDataLoader] {wrapper.monsterList.Count}개 로드 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonsterDataLoader] 예외 발생: {ex.Message}");
            }
        }
    }
}
