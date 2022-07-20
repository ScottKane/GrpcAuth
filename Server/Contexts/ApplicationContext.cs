using GrpcAuth.Server.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrpcAuth.Server.Contexts;

public class ApplicationContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    string,
    IdentityUserClaim<string>,
    IdentityUserRole<string>,
    IdentityUserLogin<string>,
    ApplicationRoleClaim,
    IdentityUserToken<string>>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ApplicationUser>(
            entity =>
            {
                entity.ToTable(
                    "Users",
                    "Identity");
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();
            });

        builder.Entity<ApplicationRole>(
            entity => entity.ToTable(
                "Roles",
                "Identity"));
        
        builder.Entity<IdentityUserRole<string>>(
            entity => entity.ToTable(
                "UserRoles",
                "Identity"));

        builder.Entity<IdentityUserClaim<string>>(
            entity => entity.ToTable(
                "UserClaims",
                "Identity"));

        builder.Entity<IdentityUserLogin<string>>(
            entity => entity.ToTable(
                "UserLogins",
                "Identity"));

        builder.Entity<ApplicationRoleClaim>(
            entity =>
            {
                entity.ToTable(
                    "RoleClaims",
                    "Identity");

                entity.HasOne(d => d.Role)
                      .WithMany(p => p.RoleClaims)
                      .HasForeignKey(d => d.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        builder.Entity<IdentityUserToken<string>>(
            entity => entity.ToTable(
                "UserTokens",
                "Identity"));
    }
}