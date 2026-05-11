namespace projektDB2.Models
{
    public class ItemSticker
    {
        public long Id { get; set; } 
        public long InventoryItemId { get; set; }
        public string? StickerName { get; set; }
        public int SlotIndex { get; set; }
        public decimal Wear { get; set; }
    }
}
