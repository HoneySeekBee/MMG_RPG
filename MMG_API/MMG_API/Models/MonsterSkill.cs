using System.ComponentModel.DataAnnotations;

namespace MMG_API.Models
{
    public class MonsterSkill
    {
        [Key]
        public int MonsterId { get; set; }
        public int SkillId { get; set; }
        public int InputType{ get; set; }
        public int Frequency { get; set; }
    }
}
