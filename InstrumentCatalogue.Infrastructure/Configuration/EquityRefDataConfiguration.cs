using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class EquityRefDataConfiguration : IEntityTypeConfiguration<EquityRefData>
{
    public void Configure(EntityTypeBuilder<EquityRefData> builder)
    {
        builder.ToTable("equity_ref_data");

        builder.HasKey(erd => erd.InstrumentId);

        builder.Property(erd => erd.LotSize).HasDefaultValue(1);

        builder.Property(erd => erd.ParValue).HasPrecision(18, 4);

        builder.Property(erd => erd.Sector).HasMaxLength(100);

        builder.Property(erd => erd.Industry).HasMaxLength(200);

        builder.Property(erd => erd.CreatedAtUtc).IsRequired();

        builder.Property(erd => erd.LastUpdatedAtUtc).IsRequired();


    }
}
