using GamePacket;
using MonsterPacket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data
{
    public class SkillDataManager
    {
        public static Dictionary<int, Skill> SkillDataDictionary = new Dictionary<int, Skill>();
        private readonly HttpClient _httpClient = new HttpClient(); 
 
        public static async Task LoadAttackData()
        {
            await LoadAllSkillsAsync(Program.URL + "/api/skill/all");
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

                Console.WriteLine($"[SkillDataManager] {SkillDataDictionary.Count}개 로드됨");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SkillDataManager] 예외 발생: {ex.Message}");
            }
        }

        public static Skill GetSkill(int id)
        {
            return SkillDataDictionary.TryGetValue(id, out var skill) ? skill : null;
        }
        public static async Task<CharacterSkillInfo> GetCharacterSkill(int characterId)
        {
            using var http = new HttpClient();
            var res = await http.GetAsync(Program.URL + $"/api/CharacterSkill/{characterId}");
            var json = await res.Content.ReadAsStringAsync();
            var CharacterSkill = JsonConvert.DeserializeObject<List<CharacterSkill>>(json);
            // characterid를 바탕으로 API를 통해 CharacterSkill에 대한 정보를 받아온다. 
            List<CharaceterSkillWithSkillData> SkillData=  new List<CharaceterSkillWithSkillData>();
            foreach (var data in CharacterSkill)
            {
                SkillData.Add(new CharaceterSkillWithSkillData(){
                    CharacterSkill = data, 
                    Skill = GetSkill(data.SkillId) });
            }
            // 이 받아온 CharacterSkill에서 SkillId들을 받는다. 
            CharacterSkillInfo result = new CharacterSkillInfo();

            result.SkillInfo.AddRange(SkillData);
            
            return result;
        }
        public static async Task<MonsterSkillInfo> GetMonsterSkill(int monsterId)
        {
            using var http = new HttpClient();
            var res = await http.GetAsync(Program.URL + $"/api/MonsterSkill/{monsterId}");
            var json = await res.Content.ReadAsStringAsync();
            var MonsterAttack = JsonConvert.DeserializeObject<List<MonsterAttack>>(json);

            List<MonsterSkill> SkillList = new List<MonsterSkill>();

            foreach(var data in MonsterAttack)
            {
                SkillList.Add(new MonsterSkill()
                {
                    MonsterAttack = data,
                    Skill = GetSkill(data.SkillId)
                });
            }

            MonsterSkillInfo result = new MonsterSkillInfo();

            result.SkillInfo.AddRange(SkillList);

            return result;
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
