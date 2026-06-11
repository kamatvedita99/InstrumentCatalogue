using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class VendorInterfaceConfiguration : IEntityTypeConfiguration<VendorInterface>
{
    public void Configure(EntityTypeBuilder<VendorInterface> builder)
    {
        builder.ToTable("vendor_interfaces");

        builder.HasKey(vi => vi.VendorInterfaceId);

        builder.Property(vi => vi.VendorInterfaceId).ValueGeneratedOnAdd();

        builder.Property(vi => vi.Name).HasMaxLength(200).IsRequired();

        builder.Property(vi => vi.IsActive).HasDefaultValue(true);

        builder.Property(vi => vi.Description).HasMaxLength(250);

        builder.Property(vi => vi.Protocol).HasMaxLength(100);

        builder.Property(vi => vi.CreatedAtUtc).IsRequired();

        builder.Property(vi => vi.LastUpdatedAtUtc).IsRequired();

        builder.HasOne(vi => vi.Vendor)
            .WithMany(v => v.Interfaces)
            .HasForeignKey(vi => vi.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<VendorInterfaceSymbolXRef>(vi => vi.SymbolXRefs)
            .WithOne()
            .HasForeignKey(visx => visx.VendorInterfaceId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(vi => new { vi.VendorId, vi.Name }).IsUnique();

    }
}
