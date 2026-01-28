using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSet for BannedIp
        public DbSet<BannedIp> BannedIp { get; set; }

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

            // Configure BannedIp entity
            builder.Entity<BannedIp>(entity =>
            {
                entity.ToTable("BannedIp");

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.BanCount)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(e => e.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500);

                entity.Property(e => e.AttemptedEmail)
                    .HasMaxLength(256);

                // Index for fast IP lookups
                entity.HasIndex(e => e.IpAddress)
                    .HasDatabaseName("IX_BannedIp_IpAddress");

                // Index for cleanup queries
                entity.HasIndex(e => e.BanExpiry)
                    .HasDatabaseName("IX_BannedIp_BanExpiry");
            });
        }
    }
}
