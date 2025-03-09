namespace Common.Infrastructure.Core.Entity;

public static class SimpleEntityRegistration
{
    public static void RegisterEntities(ModelBuilder modelBuilder, params Type[] entityTypes)
    {
        foreach (var entityType in entityTypes)
        {
            // Create the generic configuration type
            var configurationType = typeof(BaseEntityTypeConfiguration<>).MakeGenericType(entityType);

            // Create the generic interface type
            var interfaceType = typeof(IEntityTypeConfiguration<>).MakeGenericType(entityType);

            // Create instance and apply configuration using dynamic
            var configuration = Activator.CreateInstance(configurationType);
            modelBuilder.GetType()
                .GetMethod(nameof(ModelBuilder.ApplyConfiguration))
                ?.MakeGenericMethod(entityType)
                .Invoke(modelBuilder, new[] { configuration });
        }
    }
}
