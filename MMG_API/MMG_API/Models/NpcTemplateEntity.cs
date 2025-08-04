namespace MMG_API.Models
{
    public class NpcTemplateEntity
    {
        public int TemplateId { get; set; }  // PK
        public string Name { get; set; }
        public int Type { get; set; }
        public string? DialogueKey { get; set; }
        public string? JsonShopItems { get; set; }
    }
}
