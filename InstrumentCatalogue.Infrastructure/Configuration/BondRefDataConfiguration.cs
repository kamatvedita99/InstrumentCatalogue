using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class BondRefDataConfiguration : IEntityTypeConfiguration<BondRefData>
{
    public void Configure(EntityTypeBuilder<BondRefData> builder)
    {
        builder.ToTable("bond_ref_data");

        builder.HasKey(brd => brd.InstrumentId);

        builder.Property(brd => brd.FaceValue).IsRequired().HasPrecision(18,4);

        builder.Property(brd => brd.CouponRate).HasPrecision(8,6);

        builder.Property(brd => brd.Duration).HasPrecision(8,6);

        builder.Property(brd => brd.BondType).HasConversion<string>();

        builder.Property(brd=>brd.BondStructure).HasConversion<string>();

        builder.Property(brd => brd.CouponFrequency).HasConversion<string>();

        builder.Property(brd => brd.CreatedAtUtc).IsRequired();

        builder.Property(brd => brd.LastUpdatedAtUtc).IsRequired();

        builder.Property(brd => brd.Issuer).HasMaxLength(150);

        builder.Property(brd => brd.CreditRating).HasMaxLength(25);
    }
}
