namespace MMG_API.Models
{
    public class UserQuestEntity
    {
        public int CharacterId { get; set; }
        public int QuestId { get; set; }
        public byte Status { get; set; }                 // byte 또는 enum(byte) 권장
        public string? Progress { get; set; }            // JSON 문자열이면 string
        public DateTimeOffset StartedAt { get; set; }    // DB: datetimeoffset
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
