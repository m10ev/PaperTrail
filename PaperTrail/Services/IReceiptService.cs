using PaperTrail.DTO.Receipt;
using Microsoft.AspNetCore.Http;

namespace PaperTrail.Services
{
    public interface IReceiptService
    {
        Task<ReceiptDto> ScanReceiptAsync(IFormFile image);
        Task<object> SaveReceiptAsync(SaveReceiptRequest request);
        Task<IEnumerable<ReceiptResponseDto>> GetUserReceiptsAsync();
    }
}