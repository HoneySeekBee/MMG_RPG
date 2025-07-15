using System.ComponentModel.DataAnnotations;

namespace MMG_API.Models
{
    public class CharacterSkill
    {
        [Key]
        public int CharacterId { get; set; }
        public int SkillId { get; set; }
        public int InputType { get; set; }
        public int SkillLevel { get; set; }
        public int SlotIndex { get; set; }
    }
}
