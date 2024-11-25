namespace Common.Infrastructure.Data.Configurations;

public class BaseContext(DbContextOptions options) : DbContext(options);
