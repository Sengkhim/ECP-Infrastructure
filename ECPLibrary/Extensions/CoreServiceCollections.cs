using ECPLibrary.Core.Configuration;
using ECPLibrary.Core.UnitOfWork;
using ECPLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Extensions;

public static class CoreServiceCollections
{
    /// <summary>
    /// Registers the specified DbContext with the dependency injection container using the Npgsql provider,
    /// maps the context to the IEcpDatabase interface, and configures ASP.NET Core Identity services.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that implements IEcpDatabase.</typeparam>
    /// <param name="services">The IServiceCollection to which services are added.</param>
    /// <param name="config">The configuration instance used to retrieve the connection string.</param>
    /// <param name="key">The key in the configuration for the connection string.</param>
    /// <returns>The modified IServiceCollection to enable method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="services"/> or <paramref name="config"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection string corresponding to <paramref name="key"/> is null or empty.</exception>
    private static void AddDatabase<TContext>(this IServiceCollection services,
        IConfiguration config,
        string key)
        where TContext : DbContext, IEcpDatabase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        var connectionString = config.GetConnectionString(key);
        
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{key}' is null or empty.");

        // Register the DbContext using the Npgsql provider.
        services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));

        // Map the interface to the concrete DbContext.
        services.AddScoped<IEcpDatabase>(provider => provider.GetRequiredService<TContext>());

        // Configure Identity services.
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders();
    }
    
    /// <summary>
    /// Executes the provided setup action to configure database-related services on the given <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the database services will be added.</param>
    /// <param name="registration">An action delegate that performs the database service registration.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either <paramref name="services"/> or <paramref name="registration"/> is null.
    /// </exception>
    private static void AddDatabase(this IServiceCollection services, Action<IServiceCollection> registration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(registration);

        registration(services);
    }
    
    /// <summary>
    /// Registers the core ECP library services by adding the configuration modeling service as a singleton.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    private static void AddCoreEcpLibrary(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IConfigurationModeling, ConfigurationModeling>();
    }
    
    /// <summary>
    /// Registers the unit-of-work service for the specified <typeparamref name="TContext"/> with a scoped lifetime.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that implements <see cref="IEcpDatabase"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.</param>
    private static void AddUnitEcpLibrary<TContext>(this IServiceCollection services)
        where TContext : DbContext, IEcpDatabase
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
    }
    
    /// <summary>
    /// Registers the core ECP library services, database, and unit library for the specified <typeparamref name="TContext"/> 
    /// using configuration parameters to retrieve the connection string.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that implements IEcpDatabase.</typeparam>
    /// <param name="services">The IServiceCollection to which services are added.</param>
    /// <param name="configuration">The configuration instance used to retrieve the connection string.</param>
    /// <param name="key">The key used to retrieve the connection string from the configuration.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    public static void AddCoreEcpLibrary<TContext>(
        this IServiceCollection services, IConfiguration configuration, string key) 
        where TContext : DbContext, IEcpDatabase
    {
        services.AddCoreEcpLibrary();
        services.AddDatabase<TContext>(configuration, key);
        services.AddUnitEcpLibrary<TContext>();
    }
    
    /// <summary>
    /// Registers the core ECP library services, database, and unit library for the specified <typeparamref name="TContext"/> 
    /// using a custom registration action.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that implements IEcpDatabase.</typeparam>
    /// <param name="services">The IServiceCollection to which services are added.</param>
    /// <param name="registration">An action delegate that performs additional database service registrations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="registration"/> is null.
    /// </exception>
    public static void AddCoreEcpLibrary<TContext>(
        this IServiceCollection services, 
        Action<IServiceCollection> registration) 
        where TContext : DbContext, IEcpDatabase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(registration);

        services.AddCoreEcpLibrary();
        services.AddDatabase(registration);
        services.AddUnitEcpLibrary<TContext>();
    }
}