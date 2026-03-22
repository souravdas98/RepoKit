namespace AutoRepository.Dapper;

/// <summary>
/// Extension methods for registering Dapper as the repository implementation.
/// </summary>
public static class DapperServiceExtensions
{
    /// <summary>
    /// Registers Dapper as the ORM for repository implementations.
    /// Call this before <see cref="Core.ServiceCollectionExtensions.AddAutoRepositories"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionFactory">A factory function that creates <see cref="IDbConnection"/> instances from the service provider.</param>
    /// <returns>The service collection for chaining.</returns>
    ///
    /// <example>
    /// <code>
    /// services
    ///     .AddAutoRepositoryDapper(sp => new SqlConnection(connectionString))
    ///     .AddAutoRepositories(Assembly.GetExecutingAssembly());
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoRepositoryDapper(
        this IServiceCollection services,
        Func<IServiceProvider, IDbConnection> connectionFactory
    )
    {
        services.AddScoped(connectionFactory);
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
        services.AddSingleton<IRepositoryFactory>(new DapperRepositoryFactory());
        return services;
    }
}

/// <summary>
/// Internal factory for creating Dapper repository registrations.
/// </summary>
internal sealed class DapperRepositoryFactory : IRepositoryFactory
{
    /// <inheritdoc />
    public void Register(
        IServiceCollection services,
        Type entityType,
        AutoRepositoryAttribute attribute
    )
    {
        var lifetime = attribute.Lifetime;
        var customImpl = attribute.CustomImplementation;

        var repoInterface = typeof(IRepository<>).MakeGenericType(entityType);
        var readInterface = typeof(IReadRepository<>).MakeGenericType(entityType);
        var implType = customImpl ?? typeof(DapperRepository<>).MakeGenericType(entityType);

        services.Add(new ServiceDescriptor(implType, implType, lifetime));
        services.Add(
            new ServiceDescriptor(repoInterface, sp => sp.GetRequiredService(implType), lifetime)
        );
        services.Add(
            new ServiceDescriptor(readInterface, sp => sp.GetRequiredService(implType), lifetime)
        );
    }
}
