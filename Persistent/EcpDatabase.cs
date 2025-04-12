using ECPLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Persistent;

public class EcpDatabase : IdentityDbContext<IdentityUser, IdentityRole, string>, IEcpDatabase
{
    private readonly IConfigurationModeling _modeling;

    public EcpDatabase(DbContextOptions options, IConfigurationModeling modeling)
        : base(options)
    {
        _modeling = modeling;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _modeling.Configuration(builder);
    }

    Task<int> IEcpDatabase.SaveChanges()
    {
        return Task.FromResult(SaveChanges());
    }
}
