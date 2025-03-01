using Microsoft.EntityFrameworkCore;
using NobetciEczane.Models;

namespace NobetciEczane.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EczaneModel> Eczaneler { get; set; }
    }
}
