namespace PaperTrail.Models.ReceiptModels
{
    public class ReceiptItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string ReceiptId { get; set; } = string.Empty;
        public Receipt Receipt { get; set; } = null!;

        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; } = null!;

        public double Quantity { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public bool IsWeighted { get; set; }
    }
}
