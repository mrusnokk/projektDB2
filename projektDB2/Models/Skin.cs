namespace projektDB2.Models
{
    public class Skin
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? WeaponName { get; set; }
        public string? Rarity { get; set; }
        public string? ColorHex { get; set; }
        public string? CollectionName { get; set; }
        public decimal MinFloat { get; set; }
        public decimal MaxFloat { get; set; }
    }
}
