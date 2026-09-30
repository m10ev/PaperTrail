using Microsoft.AspNetCore.Identity;
using PaperTrail.Models.ReceiptModels;

namespace PaperTrail.Models.UserModels
{
    public class User : IdentityUser
    {
        public List<Receipt> Receipts { get; set; } = new();
    }
}
