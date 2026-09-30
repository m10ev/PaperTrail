namespace PaperTrail.Models.ReceiptModels
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public List<ReceiptItem> History { get; set; } = new();
    }
}
