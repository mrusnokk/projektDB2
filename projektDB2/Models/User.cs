namespace projektDB2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? SteamId { get; set; }
        public string? Username { get; set; }
        public decimal Balance { get; set; }
    }
}
