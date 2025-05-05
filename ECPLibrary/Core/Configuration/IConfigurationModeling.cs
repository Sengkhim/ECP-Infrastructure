using System.Reflection;
using ECPLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Core.Configuration;

public sealed class ConfigurationModeling : IConfigurationModeling
{
    public void Configuration(ModelBuilder builder)
    {
        builder.Entity<IdentityUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokes");
        
        builder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
        builder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId });
        builder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
        
        var cascade = builder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(e => e.DeleteBehavior == DeleteBehavior.Cascade);
        
        foreach (var c in cascade) 
            c.DeleteBehavior = DeleteBehavior.NoAction;
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}