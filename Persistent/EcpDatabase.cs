using ECPLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Persistent;

public class EcpDatabase(DbContextOptions<EcpDatabase> options, IConfigurationModeling modeling)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options), IEcpDatabase
{
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        modeling.Configuration(builder);
    }

    Task<int> IEcpDatabase.SaveChanges()
    {
        return Task.FromResult(SaveChanges());
    }
}