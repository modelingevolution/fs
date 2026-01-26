using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Extension methods for registering file system services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default file system implementation to the service collection.
    /// </summary>
    public static IServiceCollection AddFileSystem(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        return services;
    }
}
