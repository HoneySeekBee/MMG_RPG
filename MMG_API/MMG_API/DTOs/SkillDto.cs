using MMG_API.Models;
using System.ComponentModel.DataAnnotations;

namespace MMG_API.DTOs
{
    public class SkillDto
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
    public static class SkillDtoExtensions
    {
        public static Skill ToEntity(this SkillDto dto)
        {
            return new Skill
            {
                AttackId = dto.AttackId,
                AttackName = dto.AttackName,
                OwnerType = dto.OwnerType,
                WeaponType = dto.WeaponType,
                AttackType = dto.AttackType,
                Range = dto.Range,
                Angle = dto.Angle,
                Damage = dto.Damage,
                Cooldown = dto.Cooldown,
                DelayAfter = dto.DelayAfter,
                CastTime = dto.CastTime
            };
        }
    }

}
