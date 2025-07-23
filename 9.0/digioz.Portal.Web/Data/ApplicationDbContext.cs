using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Customize the ASP.NET Identity model and override table names if needed
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUser>().ToTable("AspNetUsers");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
        }
    }
}
