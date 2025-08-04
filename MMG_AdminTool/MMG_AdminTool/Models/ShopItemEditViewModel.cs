namespace MMG_AdminTool.Models
{
    public class ShopItemEntry
    {
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
    }
    public class ShopItemEditViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public List<ItemViewModel> AvailableItems { get; set; } = new();
        public List<ShopItemEntry> CurrentShopItems { get; set; } = new();
    }
}
