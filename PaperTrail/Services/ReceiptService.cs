using Azure;
using Azure.AI.DocumentIntelligence;
using PaperTrail.DTO.Receipt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using PaperTrail.Data;
using PaperTrail.Models.ReceiptModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PaperTrail.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly DocumentIntelligenceClient _client;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReceiptService(
            IConfiguration configuration,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            string endpoint = configuration["AzureDocumentIntelligence:Endpoint"]
                ?? throw new InvalidOperationException("Azure Endpoint is missing in configuration.");

            string apiKey = configuration["AzureDocumentIntelligence:ApiKey"]
                ?? throw new InvalidOperationException("Azure API Key is missing in configuration.");

            _client = new DocumentIntelligenceClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReceiptDto> ScanReceiptAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Please upload a file.");

            using var stream = image.OpenReadStream();
            var content = BinaryData.FromStream(stream);

            Operation<AnalyzeResult> operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-receipt",
                content);

            AnalyzeResult result = operation.Value;

            if (result.Documents.Count == 0)
                throw new InvalidOperationException("Could not detect a receipt in the image.");

            var doc = result.Documents[0];

            string merchant = doc.Fields.TryGetValue("MerchantName", out var m) ? m.ValueString : "Unknown Merchant";

            decimal total = doc.Fields.TryGetValue("Total", out var t) && t.ValueCurrency != null
                ? (decimal)t.ValueCurrency.Amount
                : 0m;

            var items = new List<ReceiptItemDto>();

            if (doc.Fields.TryGetValue("Items", out var itemsField) && itemsField.ValueList != null)
            {
                foreach (var itemField in itemsField.ValueList)
                {
                    if (itemField.ValueDictionary != null)
                    {
                        string desc = itemField.ValueDictionary.TryGetValue("Description", out var d) ? d.ValueString : "Item";

                        if (!string.IsNullOrEmpty(desc))
                        {
                            desc = desc.Trim('^', '\\', ' ');
                        }

                        double qty = itemField.ValueDictionary.TryGetValue("Quantity", out var q) ? q.ValueDouble ?? 1.0 : 1.0;

                        decimal? unitPrice = itemField.ValueDictionary.TryGetValue("Price", out var pr) && pr.ValueCurrency != null
                            ? (decimal)pr.ValueCurrency.Amount
                            : null;

                        decimal totalPrice = itemField.ValueDictionary.TryGetValue("TotalPrice", out var tp) && tp.ValueCurrency != null
                            ? (decimal)tp.ValueCurrency.Amount
                            : (unitPrice.HasValue ? unitPrice.Value * (decimal)qty : 0m);

                        items.Add(new ReceiptItemDto
                        {
                            Description = desc,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            TotalPrice = (double)totalPrice
                        });
                    }
                }
            }

            return new ReceiptDto(merchant, total, items);
        }

        public async Task<object> SaveReceiptAsync(SaveReceiptRequest request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
                throw new ArgumentException("Invalid receipt data.");

            // 1. Get the user's email from the token (ClaimTypes.Name)
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
                ?? _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type.EndsWith("name"))?.Value;

            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("User email claim could not be found in the token.");
            }

            // 2. Find the user in the database using their email to get their actual Id
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"User with email '{email}' was not found in the database.");
            }

            // 3. Create the main receipt entity using the database User Id
            var receiptEntity = new Receipt
            {
                UserId = user.Id,
                Merchant = request.Merchant,
                Total = request.Total,
                Date = request.Date,
                Items = new List<ReceiptItem>()
            };

            // 4. Process items and link/create Products
            foreach (var dtoItem in request.Items)
            {
                string productName = string.IsNullOrWhiteSpace(dtoItem.Description) ? "Unknown Product" : dtoItem.Description;

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == productName.ToLower());

                if (product == null)
                {
                    product = new Product
                    {
                        Name = productName
                    };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                }

                var receiptItem = new ReceiptItem
                {
                    Receipt = receiptEntity,
                    ProductId = product.Id,
                    Quantity = dtoItem.Quantity,
                    UnitPrice = dtoItem.UnitPrice,
                    TotalPrice = dtoItem.TotalPrice,
                    IsWeighted = false
                };

                receiptEntity.Items.Add(receiptItem);
            }

            _context.Receipts.Add(receiptEntity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var detailedError = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Database Error: {detailedError}");
            }

            return new
            {
                Message = "Receipt and products successfully saved to your costs!",
                SavedReceiptId = receiptEntity.Id,
                SavedMerchant = request.Merchant,
                TotalItemsCount = request.Items.Count,
                FinalTotal = request.Total
            };
        }

        public async Task<IEnumerable<ReceiptResponseDto>> GetUserReceiptsAsync()
        {
            // 1. Get the user's email from the token
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
                ?? _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type.EndsWith("name"))?.Value;

            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("User email claim could not be found in the token.");
            }

            // 2. Find the user in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"User with email '{email}' was not found in the database.");
            }

            // 3. Fetch receipts belonging to this user, including their items and product details
            var receipts = await _context.Receipts
                .Where(r => r.UserId == user.Id)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            // 4. Map to DTOs
            return receipts.Select(r => new ReceiptResponseDto
            {
                Id = r.Id,
                Merchant = r.Merchant,
                Total = r.Total,
                Date = r.Date,
                Items = r.Items.Select(i => new ReceiptItemResponseDto
                {
                    Id = i.Id,
                    ProductName = i.Product?.Name ?? "Unknown Product",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    IsWeighted = i.IsWeighted
                }).ToList()
            });
        }
    }
}