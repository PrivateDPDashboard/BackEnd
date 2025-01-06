using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Vocabulary.Database.Entities
{
    public class SaleDbContext : IdentityDbContext<ApplicationUser>
    {
        public SaleDbContext() { }

        public SaleDbContext(DbContextOptions<SaleDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            //optionsBuilder.UseSqlServer("Server=MAJD-PC;Database=Vocabulary;Trusted_Connection=True;TrustServerCertificate=True");
        }

    }
}
