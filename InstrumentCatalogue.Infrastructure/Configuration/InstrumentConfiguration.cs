using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class InstrumentConfiguration: IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("instruments");

        builder.HasKey(i => i.InstrumentId);

        builder.Property(i => i.InstrumentId).ValueGeneratedNever();

        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);

        builder.Property(i => i.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Exchange).HasMaxLength(100);

        builder.Property(i => i.Currency).HasMaxLength(25);

        builder.Property(i => i.Country).HasMaxLength(25);

        builder.Property(i => i.CreatedAtUtc).IsRequired();

        builder.Property(i => i.LastUpdatedAtUtc).IsRequired();

        builder
            .HasOne<EquityRefData>( i => i.EquityRefData)
            .WithOne()
            .HasForeignKey<EquityRefData>( erd => erd.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<BondRefData>(i => i.BondRefData)
          .WithOne()
          .HasForeignKey<BondRefData>(brd => brd.InstrumentId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<EtfRefData>(i => i.EtfRefData)
          .WithOne()
          .HasForeignKey<EtfRefData>(erd => erd.InstrumentId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<InstrumentStatusHistory>(i => i.StatusHistory)
                .WithOne()
                .HasForeignKey(ish => ish.InstrumentId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SymbolXRef>(i => i.Symbols)
            .WithOne(sxr => sxr.Instrument)
            .HasForeignKey(sxr => sxr.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.Type);

        builder.HasIndex(i => new { i.CreatedAtUtc, i.InstrumentId })
       .IsDescending(true, true);

    }
}