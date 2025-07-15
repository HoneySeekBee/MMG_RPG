using MMG_API.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MMG_API.Models
{
    public class Skill
    {
        [Key]
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
