using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.ApiDocumentation;
using Common.Infrastructure.Repository.Implementations;
using Common.Infrastructure.Repository.Interfaces;
using Common.TestApi.Data;
using Common.TestApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reports.Application.Interfaces;
using Reports.Application.Services;
using Scrutor;
using Services.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


string[] nameSpaces =
[
    "Common.Infrastructure.Repository.Implementations",
    "Reports.Application.Services",
    "FactElect.Application.Services",
            "Common.TestApi.Services",
            "Common.Core.Security.Services"
];

builder.Services.Scan(scan => scan
    .FromApplicationDependencies()
    .AddClasses(classes => classes.InNamespaces(nameSpaces))
    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
    .AsImplementedInterfaces()
    .WithTransientLifetime()
);

builder.Services.AddDataProtection();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddVersioningAndSwagger();

builder.Services.AddDbContextPool<TestContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>((hostContext, container) =>
    {
        container.RegisterType<AdoSqlServer>().As<IAdoSqlServer>();
    });




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerAndVersioning();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
