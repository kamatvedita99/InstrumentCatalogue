using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class InstrumentStatusHistoryConfiguration : IEntityTypeConfiguration<InstrumentStatusHistory>
{
    public void Configure(EntityTypeBuilder<InstrumentStatusHistory> builder)
    {
        builder.ToTable("instrument_status_history");

        builder.HasKey(ish => ish.InstrumentStatusHistoryId);

        builder.Property(ish => ish.ValidFrom).IsRequired();

        builder.Property(ish => ish.ValidTo)
                                    .IsRequired()
                                    .HasDefaultValueSql("'9999-12-31'::date");

        builder.Property(ish => ish.EffectiveDate).IsRequired();

        builder.Property(ish => ish.InstrumentStatus).IsRequired().HasConversion<string>();

        builder.Property(ish => ish.Notes).HasMaxLength(500);

        builder.Property(ish => ish.CreatedAtUtc).IsRequired();

        builder.Property(ish => ish.LastUpdatedAtUtc).IsRequired();

        builder.HasIndex(ish => new { ish.InstrumentId, ish.ValidFrom }).IsUnique();

        builder.HasIndex(sxr => sxr.InstrumentId)
               .HasFilter("valid_to = '9999-12-31'")
               .HasDatabaseName("idx_status_current");
    }
}
