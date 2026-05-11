namespace projektDB2.Models
{
    public class InventoryItem
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string? SkinName { get; set; }
        public int SkinId { get; set; }
        public string? WeaponName { get; set; }
        public string? Rarity { get; set; }
        public string? ColorHex { get; set; }
        public decimal FloatValue { get; set; }
        public int PatternSeed { get; set; }
        public bool IsStatTrak { get; set; }
        public int StickerCount { get; set; }
    }
}
