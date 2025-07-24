namespace MMG_API.DTOs
{
    public class CreateCharacterDto
    {
        public int SlotNumber { get; set; }
        public string CharacterName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public int Class { get; set; }
        public string AppearanceCode { get; set; } = string.Empty;
    }
    public class CharacterSummaryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SlotNumber { get; set; }
        public int Gender { get; set; }
        public string CharacterName { get; set; } = string.Empty;
        public int Class { get; set; }
        public string AppearanceCode { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? LastPlayedAt { get; set; } = string.Empty;
        public int? LastMapId { get; set; }
        public int? LastSpawnPointId { get; set; }
        public bool IsDeleted { get; set; }
        public int Level { get; set; }
    }
}