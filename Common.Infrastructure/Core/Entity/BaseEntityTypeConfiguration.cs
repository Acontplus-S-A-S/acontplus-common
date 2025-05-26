using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Infrastructure.Core.Entity;

public class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(x => x.Enabled).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
