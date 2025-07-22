using System.ComponentModel.DataAnnotations;

namespace MMG_API.Models
{
    public class Monster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public float HP { get; set; }
        public float Speed { get; set; }
        public float ChaseRange { get; set; }
        public float AttackRange { get; set; }
        public float Exp{ get; set; }
        public int Gold { get; set; }
    }
}
