using MMG_API.Models;

namespace MMG_API.DTOs
{
    public class CharacterSkillDto
    {
        public int CharacterId { get; set; }
        public int SkillId { get; set; }
        public int InputType { get; set; }
        public int SkillLevel { get; set; }
        public int SlotIndex { get; set; }


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
