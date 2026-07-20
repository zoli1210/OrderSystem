using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Repository.Persistence.Configuration;

public class EmailNotificationHistoryConfiguration
    : IEntityTypeConfiguration<EmailNotificationHistory>
{
    public void Configure(EntityTypeBuilder<EmailNotificationHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();

        builder.Property(x => x.Recipient).IsRequired().HasMaxLength(320);

        builder.Property(x => x.Subject).IsRequired().HasMaxLength(500);

        builder.Property(x => x.Body).IsRequired();

        builder.Property(x => x.EmailType).IsRequired().HasMaxLength(100).HasDefaultValue("Legacy");

        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property(x => x.SentAtUtc);

        builder.Property(x => x.FailedAtUtc);

        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasIndex(x => new { x.OrderId, x.EmailType }).IsUnique().HasFilter("[Status] = 2");
    }
}
