using System.Reflection;
using Common.Infrastructure.Context;
using Common.Infrastructure.Entity;
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
