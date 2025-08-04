namespace MMG_AdminTool.Models
{
    public class NpcTemplateViewModel
    {
        public int TemplateId { get; set; } // Auto Increment
        public string Name { get; set; }
        public int Type { get; set; }
        public string? DialogueKey { get; set; }
        public string? JsonShopItems { get; set; }
    }
}
