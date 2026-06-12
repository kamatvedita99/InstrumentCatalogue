using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class EtfRefDataConfiguration : IEntityTypeConfiguration<EtfRefData>
{
    public void Configure(EntityTypeBuilder<EtfRefData> builder)
    {
        builder.ToTable("etf_ref_data");

        builder.HasKey(erd => erd.InstrumentId);

        builder.Property(erd => erd.ReplicationType).HasConversion<string>();

        builder.Property(erd => erd.DistributionFrequency).HasConversion<string>();

        builder.Property(erd => erd.ExpenseRatio).HasPrecision(8,6);

        builder.Property(erd => erd.FundManager).HasMaxLength(300);

        builder.Property(erd => erd.UnderlyingIndex).HasMaxLength(200);

        builder.Property(erd => erd.CreatedAtUtc).IsRequired();

        builder.Property(erd => erd.LastUpdatedAtUtc).IsRequired();
    }

}
