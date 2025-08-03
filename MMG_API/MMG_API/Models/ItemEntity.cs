namespace MMG_API.Models
{
    public class ItemEntity
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int? IconId { get; set; }
        public int? ModelId { get; set; }
        public int? RequiredLevel { get; set; }
        public string? JsonStatModifiers { get; set; }
        public string? JsonRequiredStats { get; set; }
        public string? JsonUseableEffect { get; set; }
    }
}
