using PaperTrail.Models.UserModels;

namespace PaperTrail.Models.ReceiptModels
{
    public class Receipt
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public List<ReceiptItem> Items { get; set; } = new();
        public User Owner { get; set; } = null!;
    }
}
