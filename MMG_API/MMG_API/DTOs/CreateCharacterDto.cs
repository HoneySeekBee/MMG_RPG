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
}
