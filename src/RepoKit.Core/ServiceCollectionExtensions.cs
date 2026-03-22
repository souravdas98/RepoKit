using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRepository.Core;

/// <summary>
/// Extension methods for registering repositories in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Scans the specified assemblies for types decorated with <see cref="AutoRepositoryAttribute"/>
    /// and automatically registers <see cref="IRepository{T}"/> and <see cref="IReadRepository{T}"/> for each.
    ///
    /// Must be called after calling either <c>AddAutoRepositoryEfCore&lt;TContext&gt;()</c> or <c>AddAutoRepositoryDapper()</c>.
    /// </summary>
    /// <param name="services">The service collection to register repositories into.</param>
    /// <param name="assemblies">The assemblies to scan. If empty, scans the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no repository factory has been registered (missing AddAutoRepositoryEfCore or AddAutoRepositoryDapper call).</exception>
    ///
    /// <example>
    /// <code>
    /// services
    ///     .AddAutoRepositoryEfCore&lt;AppDbContext&gt;()
    ///     .AddAutoRepositories(Assembly.GetExecutingAssembly());
    /// </code>
    /// </example>
    public static IServiceCollection AddAutoRepositories(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        if (assemblies.Length == 0)
            assemblies = new[] { Assembly.GetCallingAssembly() };

        var repositoryFactory = GetRepositoryFactory(services);

        foreach (var assembly in assemblies)
        {
            var entityTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && t.GetCustomAttribute<AutoRepositoryAttribute>() != null
                );

            foreach (var entityType in entityTypes)
            {
                var attr = entityType.GetCustomAttribute<AutoRepositoryAttribute>()!;
                repositoryFactory.Register(services, entityType, attr);
            }
        }

        return services;
    }

    private static IRepositoryFactory GetRepositoryFactory(IServiceCollection services)
    {
        // The EF Core / Dapper package registers its factory during AddAutoRepositoryEfCore()
        // or AddAutoRepositoryDapper(). If neither is called we throw a helpful message.
        var factoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IRepositoryFactory)
        );
        if (factoryDescriptor?.ImplementationInstance is IRepositoryFactory factory)
            return factory;

        throw new InvalidOperationException(
            "No repository factory registered. Call services.AddAutoRepositoryEfCore<TContext>() "
                + "or services.AddAutoRepositoryDapper() before AddAutoRepositories()."
        );
    }
}

/// <summary>
/// Internal contract that ORM-specific implementations (EF Core, Dapper) use to register repositories.
/// Do not implement this interface directly; use the provided extension methods instead.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>
    /// Registers a repository for the specified entity type in the service collection.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="entityType">The entity type for which to register a repository.</param>
    /// <param name="attribute">The AutoRepository attribute configuration for this entity.</param>
    void Register(IServiceCollection services, Type entityType, AutoRepositoryAttribute attribute);
}
