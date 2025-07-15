namespace MMG_API.DTOs
{
    public class MonsterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float HP{ get; set; }
        public float Speed { get; set; }
        public float ChaseRange { get; set; }
        public float AttackRange { get; set; }

    }
    public static class MonsterDtoExtensions
    {
        public static Models.Monster ToEntity(this MonsterDto dto)
        {
            return new Models.Monster
            {
                Id = dto.Id,
                Name = dto.Name,
                HP = dto.HP,
                Speed = dto.Speed,
                ChaseRange = dto.ChaseRange,
                AttackRange = dto.AttackRange
            };
        }
    }
}
