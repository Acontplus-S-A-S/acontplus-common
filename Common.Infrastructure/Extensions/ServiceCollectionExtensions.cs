using Common.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Common.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the DbContext and its corresponding UnitOfWork implementation, with optional keying.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">The options for configuring the DbContext.</param>
    /// <param name="serviceKey">Optional key to register the services with.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDbContextWithUnitOfWork<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction,
        object? serviceKey = null) // Aceptamos una clave de servicio opcional
        where TContext : DbContext
    {
        // Registra el DbContext usando el pool de conexiones con la clave proporcionada.
        services.AddDbContextPool<TContext>(optionsAction, poolSize: 128); // El tamaño del pool es configurable

        // Registra IUnitOfWork y DbContext con clave si se proporciona una.
        if (serviceKey is not null)
        {
            services.TryAddKeyedScoped<IUnitOfWork, UnitOfWork<TContext>>(serviceKey);
            services.TryAddKeyedScoped<DbContext>(serviceKey, (sp, key) => sp.GetRequiredKeyedService<TContext>(key));
        }
        else
        {
            services.TryAddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        }

        return services;
    }
}
