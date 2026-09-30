namespace PaperTrail.DTO.Receipt
{
    public record ReceiptDto(
        string Merchant,
        decimal Total,
        List<ReceiptItemDto> Items);

    public class ReceiptItemDto
    {
        public string Description { get; set; } = string.Empty;
        public double Quantity { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public bool IsWeighted => Quantity > 0 && Quantity != Math.Floor(Quantity);
    }

    public class SaveReceiptRequest
    {
        public string Merchant { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<ReceiptItemDto> Items { get; set; } = new();
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

    public class ReceiptResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
        public List<ReceiptItemResponseDto> Items { get; set; } = new();
    }

    public class ReceiptItemResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public bool IsWeighted { get; set; }
    }
}
