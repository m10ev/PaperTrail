using PaperTrail.DTO.Receipt;
using PaperTrail.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ReceiptController : ControllerBase
{
    private readonly IReceiptService _receiptService;

    public ReceiptController(IReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyReceipts()
    {
        try
        {
            var receipts = await _receiptService.GetUserReceiptsAsync();
            return Ok(receipts);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving receipts: {ex.Message}");
        }
    }

    [HttpPost("scan")]
    public async Task<IActionResult> ScanReceipt(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("Please upload a file.");

        try
        {
            var parsedReceipt = await _receiptService.ScanReceiptAsync(image);
            return Ok(parsedReceipt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Azure OCR Error: {ex.Message}");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveReceipt([FromBody] SaveReceiptRequest request)
    {
        try
        {
            var result = await _receiptService.SaveReceiptAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Save Error: {ex.Message}");
        }
    }
}