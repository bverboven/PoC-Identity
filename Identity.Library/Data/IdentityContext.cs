using Identity.Library.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Library.Data
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LoginEntry> LoginEntries { get; set; }


        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            var refreshToken = builder.Entity<RefreshToken>();
            refreshToken
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .HasConstraintName("fk_refreshtoken_userid");
            var loginEntry = builder.Entity<LoginEntry>();
            loginEntry
                .HasIndex(e => e.UserId);
            loginEntry
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("fk_loginentry_userid");
        }
    }
}
