using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MMG_API.Models;

namespace MMG_API.DTOs
{
    public class MonsterSkillDto
    {
        public int MonsterId { get; set; }
        public int SkillId { get; set; }
        public int Frequency { get; set; }
        public int InputType { get; set; }
    }

    public class MonsterWithSkillsDto
    {
        public int MonsterId { get; set; }
        public string Name { get; set; }
        public float HP { get; set; }
        public float Speed { get; set; }
        public float ChaseRange { get; set; }
        public float AttackRange { get; set; }
        public float Exp{ get; set; }
        public int Gold{ get; set; }

        public List<MonsterSkillDto> Skills { get; set; } = new();
    }
    public static class MonsterSkillDtoExtensions
    {
        public static MonsterSkill ToEntity(this MonsterSkillDto dto)
        {
            return new MonsterSkill
            {
                MonsterId = dto.MonsterId,
                SkillId = dto.SkillId,
                Frequency = dto.Frequency,
                InputType = dto.InputType,
            };
        }
    }
}
