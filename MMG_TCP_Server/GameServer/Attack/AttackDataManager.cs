using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Data;
using Newtonsoft.Json;


namespace GameServer.Attack
{
    public class AttackDataManager
    {
        private static Dictionary<int, AttackData> _attackDataDict = new();

        public static AttackData Get(int attackId)
        {
            return _attackDataDict.TryGetValue(attackId, out var data) ? data : null;
        }

        public static void LoadAttackData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string rootDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..")); // GameServer 프로젝트 루트
            try
            {
                string fullPath = Path.Combine(rootDir, "Resources", "AttackData", "attack_data.json");

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"[AttackDataLoader] 파일 없음: {fullPath}");
                    return;
                }

                string json = File.ReadAllText(fullPath);
                var wrapper = JsonConvert.DeserializeObject<AttackDataListWrapper>(json);

                if (wrapper == null || wrapper.attackList == null)
                {
                    Console.WriteLine("[AttackDataLoader] JSON 구조가 올바르지 않음");
                    return;
                }

                _attackDataDict = wrapper.attackList.ToDictionary(x => x.AttackId, x => x);
                Console.WriteLine($"[AttackDataLoader] {wrapper.attackList.Count}개 로드 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttackDataLoader] 예외 발생: {ex.Message}");
            }
        }
    }
}
