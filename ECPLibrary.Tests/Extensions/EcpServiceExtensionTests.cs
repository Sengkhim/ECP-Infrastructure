using ECPLibrary.Core.UnitOfWork;
using ECPLibrary.Extensions;
using ECPLibrary.Services;
using ECPLibrary.tests;
using ECPLibrary.Tests.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECPLibrary.Tests.Extensions;

public class EcpServiceExtensionTests
{
    private readonly IServiceCollection _services = new ServiceCollection();
    
    private readonly IConfiguration _validConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            {
                "ConnectionStrings:Xunit",
                "Host=localhost;Database=xunit;Username=postgres;Password=1688"
            }
        }!)
        .Build();

    [Fact]
    public void AddDatabase_Throws_When_Services_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            ((IServiceCollection)null!).AddDatabase<FakeDbContext>(_validConfig, "Xunit");
        });
    }

    [Fact]
    public void AddDatabase_Throws_When_Config_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _services.AddDatabase<FakeDbContext>(null, "Xunit");
        });
    }

    [Fact]
    public void AddDatabase_With_Valid_Config_Should_Register_Dependencies()
    {
        _services.AddDatabase<FakeDbContext>(_validConfig, "Xunit");

        var provider = _services.BuildServiceProvider();
        var db = provider.GetService<IEcpDatabase>();

        Assert.NotNull(db);
        Assert.IsType<FakeDbContext>(db);
    }

    [Fact]
    public void AddCoreEcpLibrary_Config_Overload_Should_Register_All()
    {
        _services.AddCoreEcpLibrary<FakeDbContext>(_validConfig, "Xunit");
    
        var provider = _services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IConfigurationModeling>());
        Assert.NotNull(provider.GetService<IEcpDatabase>());
        Assert.NotNull(provider.GetService<IUnitOfWork<FakeDbContext>>());
    }

     [Fact]
     public void AddCoreEcpLibrary_Delegate_Overload_Should_Register_All()
     {
         _services.AddCoreEcpLibrary<FakeDbContext>((svc) =>
         {
             svc.AddDbContext<IEcpDatabase, FakeDbContext>(opt =>
                 opt.UseNpgsql("Host=localhost;Database=xunit;Username=postgres;Password=9090"));
         });
     
         var provider = _services.BuildServiceProvider();
                  
         Assert.NotNull(provider.GetService<IConfigurationModeling>());
         Assert.NotNull(provider.GetService<IEcpDatabase>());
         Assert.NotNull(provider.GetService<IUnitOfWork<FakeDbContext>>());
     }
     
     [Fact]
     public void Add_Auto_Migrations()
     {
         _services.AddCoreEcpLibrary<InventoryDbContext>(_validConfig, "Xunit");
     
         var provider = _services.BuildServiceProvider();
         
        // Ensure scope so DbContext is correctly resolved
         using var scope = provider.CreateScope();
         var fakeContext = scope.ServiceProvider.GetService<IEcpDatabase>();

         Assert.NotNull(fakeContext);
         Assert.IsType<InventoryDbContext>(fakeContext);
         
         // Migrate the database to apply any pending migrations
         fakeContext.Database.Migrate();
     }
}
