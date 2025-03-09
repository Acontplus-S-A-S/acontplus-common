using System.Reflection;
using Common.Infrastructure.Core.Context;
using Common.Infrastructure.Core.Entity;
using Common.TestApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.TestApi.Data;


public class TestContext(DbContextOptions<TestContext> options) : BaseContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        SimpleEntityRegistration.RegisterEntities(builder, typeof(Usuario));
    }
}
