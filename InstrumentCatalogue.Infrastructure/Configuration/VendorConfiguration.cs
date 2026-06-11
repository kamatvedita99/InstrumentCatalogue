using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors");

        builder.HasKey(v => v.VendorId);

        builder.Property(v => v.VendorId).ValueGeneratedOnAdd();

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.ShortCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(v => v.Name).IsUnique();
        builder.HasIndex(v => v.ShortCode).IsUnique();

        builder.Property(v => v.IsActive).HasDefaultValue(true);

        builder.Property(v => v.CreatedAtUtc).IsRequired();

        builder.Property(v=>v.LastUpdatedAtUtc).IsRequired();
    }
}
