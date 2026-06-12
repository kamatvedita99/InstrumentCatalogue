using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstrumentCatalogue.Infrastructure.Configuration;

public class SymbologyConfiguration : IEntityTypeConfiguration<Symbology>
{
    public void Configure(EntityTypeBuilder<Symbology> builder)
    {
        builder.ToTable("symbologies");

        builder.HasKey(s => s.SymbologyId);

        builder.Property(s => s.SymbologyId).ValueGeneratedOnAdd();

        builder.Property(s => s.TypeCode).IsRequired().HasMaxLength(100);

        builder.Property(s => s.Description).HasMaxLength(250);

        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.Property(s => s.LastUpdatedAtUtc).IsRequired();

        builder.Property(s => s.IsActive).HasDefaultValue(true);

        builder.HasIndex(s => s.TypeCode).IsUnique();
        
       
    }
}
