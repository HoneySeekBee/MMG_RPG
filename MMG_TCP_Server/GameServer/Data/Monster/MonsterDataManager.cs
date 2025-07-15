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
            return _monsterDataDict.TryGetValue(monsterId, out var data) ? data : null;
        }

        public static async void LoadData()
        {
            try
            {
                using var http = new HttpClient();
                var response = await http.GetAsync($"{Program.URL}/api/monster/all-with-skills");
                var json = await response.Content.ReadAsStringAsync();

                var monsters = JsonConvert.DeserializeObject<List<MonsterDto>>(json);

                _monsterDataDict.Clear();

                foreach (var dto in monsters)
                {
                    var monsterData = new MonsterPacket.MonsterData
                    {
                        MonsterId = dto.Id,
                        MonsterName = dto.Name ?? "",
                        MaxHP = dto.HP,
                        MoveSpeed = dto.Speed,
                        ChaseRange = dto.ChaseRange,
                        AttackRange = dto.AttackRange
                    };

                    foreach (var skill in dto.Skills)
                    {
                        monsterData.Attacks.Add(new MonsterPacket.MonsterAttack
                        {
                            SkillId = skill.SkillId,
                            Frequency = skill.Frequency
                        });
                    }

                    _monsterDataDict[dto.Id] = monsterData;
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
            public int Id { get; set; }
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
