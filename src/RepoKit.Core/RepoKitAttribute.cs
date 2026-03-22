using Microsoft.Extensions.DependencyInjection;

namespace AutoRepository.Core;

/// <summary>
/// Marks an entity class for automatic repository registration.
/// The DI container will automatically get IRepository&lt;T&gt; and IReadRepository&lt;T&gt;
/// registered without any manual wiring.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AutoRepositoryAttribute : Attribute
{
    /// <summary>
    /// DI lifetime for the repository. Defaults to Scoped.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// When true, only IReadRepository&lt;T&gt; is registered (no write operations).
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// Provide your own repository implementation to override the default EF Core one.
    /// Must implement IRepository&lt;T&gt; (or IReadRepository&lt;T&gt; if ReadOnly = true).
    /// </summary>
    public Type? CustomImplementation { get; set; } = null;
}
