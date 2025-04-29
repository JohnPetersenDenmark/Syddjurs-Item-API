using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Syddjurs_Item_API.Models;

namespace Syddjurs_Item_API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 

        }

        public DbSet<Item> Items { get; set; }
        public DbSet<ItemCategory> Categories { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanItemLine> LoanItemLines { get; set; }
    }
}
