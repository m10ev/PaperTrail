using PaperTrail.Models.ReceiptModels;
using PaperTrail.Models.UserModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PaperTrail.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Receipt> Receipts { get; set; } = null!;
        public DbSet<ReceiptItem> ReceiptItems { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
