using ChallengeBank.Clients.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChallengeBank.Clients.Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.DocumentNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(c => c.DocumentNumber)
            .IsUnique();

        builder.Property(c => c.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        builder.Property(c => c.UpdatedAtUtc);
    }
}
