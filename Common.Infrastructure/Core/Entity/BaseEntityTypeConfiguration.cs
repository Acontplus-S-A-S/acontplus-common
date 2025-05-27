using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Infrastructure.Core.Entity;

public class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    private static string _cachedProvider;

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Detectar el proveedor de base de datos
        var dbProvider = GetDatabaseProvider(builder);

        // Configure CreatedAt with provider-specific default value
        ConfigureCreatedAtDefault(builder, dbProvider);

        // Configure default values for Enabled and IsActive
        builder.Property(x => x.Enabled).HasDefaultValue(true); // Deprecated field, use IsActive instead
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.FromMobile).HasDefaultValue(false);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }

    private string GetDatabaseProvider(EntityTypeBuilder<TEntity> builder)
    {
        // Cache del proveedor para evitar múltiples consultas
        if (!string.IsNullOrEmpty(_cachedProvider))
            return _cachedProvider;

        // Intentar obtener el proveedor desde las anotaciones del modelo
        var model = builder.Metadata.Model;

        // Buscar en anotaciones
        var providerAnnotation = model.FindAnnotation("Relational:ProviderName");
        if (providerAnnotation?.Value is string provider && !string.IsNullOrEmpty(provider))
        {
            _cachedProvider = provider;
            return provider;
        }

        // Fallback al proveedor estático si está configurado
        if (!string.IsNullOrEmpty(DatabaseProviderContext.CurrentProvider))
        {
            _cachedProvider = DatabaseProviderContext.CurrentProvider;
            return _cachedProvider;
        }

        // Default fallback
        _cachedProvider = "Microsoft.EntityFrameworkCore.SqlServer";
        return _cachedProvider;
    }

    private void ConfigureCreatedAtDefault(EntityTypeBuilder<TEntity> builder, string dbProvider)
    {
        switch (dbProvider)
        {
            case "Microsoft.EntityFrameworkCore.SqlServer":
                builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                break;
            case "Microsoft.EntityFrameworkCore.Sqlite":
                builder.Property(x => x.CreatedAt).HasDefaultValueSql("datetime('now')");
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
                break;
            case "Pomelo.EntityFrameworkCore.MySql":
            case "MySql.EntityFrameworkCore":
                builder.Property(x => x.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
                break;
            case "Oracle.EntityFrameworkCore":
                builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYS_EXTRACT_UTC(SYSTIMESTAMP)");
                break;
            default:
                builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                break;
        }
    }
}

// Contexto estático para el proveedor de base de datos
public static class DatabaseProviderContext
{
    public static string CurrentProvider { get; set; }

    public static void SetProvider(string providerName)
    {
        CurrentProvider = providerName;
    }

    public static void SetProvider(DbContext context)
    {
        CurrentProvider = context.Database.ProviderName;
    }
}
