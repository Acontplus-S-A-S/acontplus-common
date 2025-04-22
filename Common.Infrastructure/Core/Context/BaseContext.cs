namespace Common.Infrastructure.Core.Context;

public class SqlServerModelBuilderOptions
{
    public bool EnableDecimalConversion { get; set; } = true;
    public bool EnableNonUnicodeStrings { get; set; } = true;
}

public class BaseContext(DbContextOptions options) : DbContext(options)
{
    protected SqlServerModelBuilderOptions SqlServerOptions { get; } = new SqlServerModelBuilderOptions();

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Modified)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;
            }
        }

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Modified)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuraciones SQL Server (solo si está habilitado)
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            ApplySqlServerConfigurations(builder);
        }
    }

    protected virtual void ApplySqlServerConfigurations(ModelBuilder builder)
    {
        // Convertir decimales a double (opcional)
        if (SqlServerOptions.EnableDecimalConversion)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(decimal));
                foreach (var property in properties)
                    builder.Entity(entityType.Name).Property(property.Name).HasConversion<double>();
            }
        }

        // Forzar columnas string a non-unicode (opcional)
        if (SqlServerOptions.EnableNonUnicodeStrings)
        {
            foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(
                         p => p.ClrType == typeof(string) && p.GetColumnType() == null
                     ))
                property.SetIsUnicode(false);
        }
    }
};

