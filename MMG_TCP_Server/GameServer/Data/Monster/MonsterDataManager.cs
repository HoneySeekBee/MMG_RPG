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
        private static Dictionary<int, MonsterData> _monsterDataDict = new();


        public static MonsterData Get(int monsterId)
        {
            Console.WriteLine($"[MonsterDataManager] {monsterId} : {_monsterDataDict.ContainsKey(monsterId)}");
            return _monsterDataDict.TryGetValue(monsterId, out var data) ? data : null;
        }

        public static async Task LoadData()
        {
            try
            {
                using var http = new HttpClient();
                var response = await http.GetAsync($"{Program.URL}/api/monster/all");
                var json = await response.Content.ReadAsStringAsync();

                var monsters = JsonConvert.DeserializeObject<List<MonsterDto>>(json);

                _monsterDataDict.Clear();

                foreach (var dto in monsters)
                {
                    var monsterData = new MonsterPacket.MonsterData
                    {
                        MonsterId = dto.MonsterId,
                        MonsterName = dto.Name ?? "",
                        MaxHP = dto.HP,
                        MoveSpeed = dto.Speed,
                        ChaseRange = dto.ChaseRange,
                        AttackRange = dto.AttackRange
                    };

                    monsterData.SkillInfo = await SkillDataManager.GetMonsterSkill(dto.MonsterId);

                    Console.WriteLine($"[MonsterDataManager] LoadData {dto.MonsterId} ");
                    _monsterDataDict[dto.MonsterId] = monsterData;
                }

                Console.WriteLine($"[MonsterDataManager] 몬스터 {monsters.Count}개 로드 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonsterDataManager] 예외 발생: {ex.Message}");
            }
        }
        public class MonsterDto
        {
            public int MonsterId { get; set; }
            public string Name { get; set; }
            public float HP { get; set; }
            public float Speed { get; set; }
            public float ChaseRange { get; set; }
            public float AttackRange { get; set; }

            public List<MonsterSkillDto> Skills { get; set; }
        }

        public class MonsterSkillDto
        {
            public int SkillId { get; set; }
            public int Frequency { get; set; }
        }
    }
}
