using Microsoft.EntityFrameworkCore;
using Syddjurs_Item_API.Models;

namespace Syddjurs_Item_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 

        }

        public DbSet<ItemCategory> Categories { get; set; }
    }
}
