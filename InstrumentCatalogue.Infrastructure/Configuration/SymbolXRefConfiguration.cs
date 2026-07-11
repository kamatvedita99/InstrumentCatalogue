using InstrumentCatalogue.Core.Constants;
using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class SymbolXRefConfiguration : IEntityTypeConfiguration<SymbolXRef>
{
    public void Configure(EntityTypeBuilder<SymbolXRef> builder)
    {
        builder.ToTable("symbol_x_ref");

        builder.HasKey(sxr => sxr.SymbolXRefId);

        builder.Property(sxr => sxr.SymbolXRefId).ValueGeneratedNever();

        builder.HasIndex(sxr => new { sxr.InstrumentId, sxr.SymbologyId, sxr.ValidFrom }).IsUnique();

        builder.HasIndex(sxr => new { sxr.SymbologyId, sxr.Symbol })
            .IsUnique()
            .HasFilter($"valid_to = '{TemporalDefaults.CurrentSentinelSql}'")
            .HasDatabaseName("ix_symbol_x_ref_symbology_id_symbol_current");

        builder.Property(sxr => sxr.ValidFrom).IsRequired();

        builder.HasOne<Symbology>(sxr => sxr.Symbology)
               .WithMany()
               .HasForeignKey(sxr => sxr.SymbologyId)
               .OnDelete(DeleteBehavior.Restrict);


        builder.HasMany<VendorInterfaceSymbolXRef>(sxr => sxr.VendorInterfaceSymbols)
            .WithOne()
            .HasForeignKey(visxr => visxr.SymbolXRefId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sxr => sxr.CreatedAtUtc).IsRequired();

        builder.Property(sxr => sxr.LastUpdatedAtUtc).IsRequired();

        builder.Property(sxr => sxr.ValidTo)
                                    .IsRequired()
                                    .HasDefaultValueSql($"'{TemporalDefaults.CurrentSentinelSql}'::date");
                                    

        builder.Property(sxr => sxr.Symbol).IsRequired().HasMaxLength(250);

        builder.HasIndex(sxr => sxr.Symbol)
               .HasFilter($"valid_to = '{TemporalDefaults.CurrentSentinelSql}'")
               .HasDatabaseName("idx_symbol_x_ref_current");

        builder.HasIndex(sxr => sxr.InstrumentId)
            .IsUnique()
            .HasFilter($"is_primary = true AND valid_to = '{TemporalDefaults.CurrentSentinelSql}'")
            .HasDatabaseName("idx_symbol_x_ref_one_primary_per_instrument_symbol");

    }
}
