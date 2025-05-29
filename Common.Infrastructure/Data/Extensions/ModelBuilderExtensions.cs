using Common.Infrastructure.Entity;

namespace Common.Infrastructure.Data.Extensions;

public static class ModelBuilderExtensions
{
    public static void RegisterSimpleEntities(this ModelBuilder modelBuilder, DbContext context, params Type[] entityTypes)
    {
        SimpleEntityRegistration.RegisterEntities(modelBuilder, context, entityTypes);
    }

    public static void RegisterSimpleEntities(this ModelBuilder modelBuilder, string providerName, params Type[] entityTypes)
    {
        DatabaseProviderContext.SetProvider(providerName);
        SimpleEntityRegistration.RegisterEntities(modelBuilder, entityTypes);
    }
}
