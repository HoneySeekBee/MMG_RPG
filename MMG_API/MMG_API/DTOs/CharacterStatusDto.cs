namespace MMG_API.DTOs
{
    public class CharacterStatusDto
    {
        public int CharacterId { get; set; }
        public int CharacterLevel { get; set; }
        public float Exp { get; set; }
        public int Gold { get; set; }
        public float HP { get; set; }
        public float MP { get; set; }
        public float NowHP { get; set; }
        public float NowMP { get; set; }
        public DateTime LastUpdateAt { get; set; }
    }
}
