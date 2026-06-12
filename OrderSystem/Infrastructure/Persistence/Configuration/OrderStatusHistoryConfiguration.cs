using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();

        builder.Property(x => x.FromStatus).IsRequired();

        builder.Property(x => x.ToStatus).IsRequired();

        builder.Property(x => x.ChangedAtUtc).IsRequired();

        builder.Property(x => x.ChangedByUserId).IsRequired().HasMaxLength(450);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ChangedAtUtc);
    }
}
