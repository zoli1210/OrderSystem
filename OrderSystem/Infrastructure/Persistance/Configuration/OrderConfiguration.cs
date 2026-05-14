using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);

        builder.Property(x => x.CreatedByUserId).IsRequired().HasMaxLength(450);

        builder.Property(x => x.CustomerEmail).IsRequired().HasMaxLength(320);

        builder.Property(x => x.EmailSentAtUtc);

        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
