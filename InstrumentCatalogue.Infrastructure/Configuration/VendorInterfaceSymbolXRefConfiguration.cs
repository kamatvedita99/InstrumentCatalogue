using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class VendorInterfaceSymbolXRefConfiguration : IEntityTypeConfiguration<VendorInterfaceSymbolXRef>
{
    public void Configure(EntityTypeBuilder<VendorInterfaceSymbolXRef> builder)
    {
        builder.ToTable("vendor_interface_symbol_x_ref");

        builder.HasKey(visxr => visxr.VendorInterfaceSymbolXRefId);

        builder.Property(visxr => visxr.VendorInterfaceSymbolXRefId).ValueGeneratedNever();

        builder.Property(visxr => visxr.CreatedAtUtc).IsRequired();

        builder.Property(visxr => visxr.LastUpdatedAtUtc).IsRequired();

        builder.Property(visxr => visxr.ReceivedAtUtc).IsRequired();

        builder.Property(visxr => visxr.IsActive).HasDefaultValue(true);

        builder.HasIndex(visxr => new{ visxr.VendorInterfaceId, visxr.SymbolXRefId }).IsUnique();

    }
}
