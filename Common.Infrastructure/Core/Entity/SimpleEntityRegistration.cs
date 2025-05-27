namespace Common.Infrastructure.Core.Entity;

public static class SimpleEntityRegistration
{
    public static void RegisterEntities(ModelBuilder modelBuilder, params Type[] entityTypes)
    {
        // Asegurar que el proveedor esté configurado
        EnsureProviderIsSet(modelBuilder);

        foreach (var entityType in entityTypes)
        {
            // Create the generic configuration type
            var configurationType = typeof(BaseEntityTypeConfiguration<>).MakeGenericType(entityType);

            // Create instance and apply configuration
            var configuration = Activator.CreateInstance(configurationType);

            modelBuilder.GetType()
                .GetMethod(nameof(ModelBuilder.ApplyConfiguration))
                ?.MakeGenericMethod(entityType)
                .Invoke(modelBuilder, new[] { configuration });
        }
    }

    public static void RegisterEntities(ModelBuilder modelBuilder, DbContext context, params Type[] entityTypes)
    {
        // Configurar el proveedor desde el contexto
        DatabaseProviderContext.SetProvider(context);
        RegisterEntities(modelBuilder, entityTypes);
    }

    private static void EnsureProviderIsSet(ModelBuilder modelBuilder)
    {
        if (string.IsNullOrEmpty(DatabaseProviderContext.CurrentProvider))
        {
            // Intentar detectar desde el modelo
            var providerAnnotation = modelBuilder.Model.FindAnnotation("Relational:ProviderName");
            if (providerAnnotation?.Value is string provider)
            {
                DatabaseProviderContext.CurrentProvider = provider;
            }
        }
    }
}
