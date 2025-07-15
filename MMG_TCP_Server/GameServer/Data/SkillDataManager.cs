using GamePacket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class SkillDataManager
    {
        public static Dictionary<int, Skill> SkillDataDictionary = new Dictionary<int, Skill>();
        private readonly HttpClient _httpClient = new HttpClient(); 
        public static void LoadAttackData()
        {
            var task = LoadAllSkillsAsync(Program.URL + "/api/skill/all");
            task.Wait(); // 동기처럼 대기
        }
        public static async Task LoadAllSkillsAsync(string apiUrl)
        {
            try
            {
                using var http = new HttpClient();
                var res = await http.GetAsync(apiUrl);
                var json = await res.Content.ReadAsStringAsync();
                var skillDtos = JsonConvert.DeserializeObject<List<SkillDto>>(json);

                SkillDataDictionary.Clear();
                foreach (var dto in skillDtos)
                {
                    SkillDataDictionary[dto.AttackId] = new Skill
                    {
                        AttackId = dto.AttackId,
                        AttackName = dto.AttackName ?? "",
                        OwnerType = (OwnerType)dto.OwnerType,
                        WeaponType = (AttackPacket.WeaponType)dto.WeaponType,
                        AttackType = (AttackPacket.AttackType)dto.AttackType,
                        Range = dto.Range,
                        Angle = dto.Angle,
                        Damage = dto.Damage,
                        Cooldown = dto.Cooldown,
                        DelayAfter = dto.DelayAfter,
                        CastTime = dto.CastTime
                    };
                }

                Console.WriteLine($"[AttackDataManager] {SkillDataDictionary.Count}개 로드됨");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttackDataManager] 예외 발생: {ex.Message}");
            }
        }

        public static Skill GetSkill(int id)
        {
            return SkillDataDictionary.TryGetValue(id, out var skill) ? skill : null;
        }

        // SkillDto 내부 정의 (API용)
        private class SkillDto
        {
            public int AttackId { get; set; }
            public string AttackName { get; set; }
            public int OwnerType { get; set; }
            public int WeaponType { get; set; }
            public int AttackType { get; set; }
            public float Range { get; set; }
            public float Angle { get; set; }
            public float Damage { get; set; }
            public float Cooldown { get; set; }
            public float DelayAfter { get; set; }
            public float CastTime { get; set; }
        }
    }
}
