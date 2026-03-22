using AutoRepository.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRepository.EfCore;

/// <summary>
/// Call services.AddAutoRepositoryEfCore&lt;AppDbContext&gt;() in Program.cs,
/// then services.AddAutoRepositories() — that's all.
/// </summary>
public static class EfCoreServiceExtensions
{
    public static IServiceCollection AddAutoRepositoryEfCore<TContext>(
        this IServiceCollection services
    )
        where TContext : DbContext
    {
        // Register DbContext as the base class so our generic repo can accept it
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Register the factory singleton so the scanner knows how to wire things up
        services.AddSingleton<IRepositoryFactory>(new EfRepositoryFactory(typeof(TContext)));

        return services;
    }
}

internal sealed class EfRepositoryFactory : IRepositoryFactory
{
    private readonly Type _contextType;

    public EfRepositoryFactory(Type contextType)
    {
        _contextType = contextType;
    }

    public void Register(
        IServiceCollection services,
        Type entityType,
        AutoRepositoryAttribute attribute
    )
    {
        var lifetime = attribute.Lifetime;
        var readOnly = attribute.ReadOnly;
        var customImpl = attribute.CustomImplementation;

        if (readOnly)
        {
            RegisterReadOnly(services, entityType, customImpl, lifetime);
        }
        else
        {
            RegisterReadWrite(services, entityType, customImpl, lifetime);
        }
    }

    private static void RegisterReadOnly(
        IServiceCollection services,
        Type entityType,
        Type? customImpl,
        ServiceLifetime lifetime
    )
    {
        var interfaceType = typeof(IReadRepository<>).MakeGenericType(entityType);
        var implType = customImpl ?? typeof(EfRepository<>).MakeGenericType(entityType);

        ValidateImplementation(implType, interfaceType, entityType);
        services.Add(new ServiceDescriptor(interfaceType, implType, lifetime));
    }

    private static void RegisterReadWrite(
        IServiceCollection services,
        Type entityType,
        Type? customImpl,
        ServiceLifetime lifetime
    )
    {
        var repoInterface = typeof(IRepository<>).MakeGenericType(entityType);
        var readInterface = typeof(IReadRepository<>).MakeGenericType(entityType);
        var implType = customImpl ?? typeof(EfRepository<>).MakeGenericType(entityType);

        ValidateImplementation(implType, repoInterface, entityType);

        // Register the concrete type itself
        services.Add(new ServiceDescriptor(implType, implType, lifetime));

        // IRepository<T> and IReadRepository<T> both resolve to the same instance
        services.Add(
            new ServiceDescriptor(repoInterface, sp => sp.GetRequiredService(implType), lifetime)
        );
        services.Add(
            new ServiceDescriptor(readInterface, sp => sp.GetRequiredService(implType), lifetime)
        );
    }

    private static void ValidateImplementation(Type implType, Type interfaceType, Type entityType)
    {
        if (!interfaceType.IsAssignableFrom(implType))
        {
            throw new InvalidOperationException(
                $"Custom implementation '{implType.Name}' does not implement '{interfaceType.Name}' "
                    + $"for entity '{entityType.Name}'."
            );
        }
    }
}
