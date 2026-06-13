using InstrumentCatalogue.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InstrumentCatalogue.Infrastructure.Persistence;

public class CatalogueDbContext: DbContext
{
    public CatalogueDbContext(DbContextOptions<CatalogueDbContext> options): base(options)
    {
        
    }

    public DbSet<Instrument> Instruments => Set<Instrument>();

    public DbSet<InstrumentStatusHistory> InstrumentStatusHistory => Set<InstrumentStatusHistory>();

    public DbSet<Symbology> Symbologies => Set<Symbology>();

    public DbSet<Vendor> Vendors => Set<Vendor>();

    public DbSet<VendorInterface> VendorInterfaces => Set<VendorInterface>();

    public DbSet<SymbolXRef> SymbolXRefs => Set<SymbolXRef>();

    public DbSet<VendorInterfaceSymbolXRef> VendorInterfaceSymbolXRefs => Set<VendorInterfaceSymbolXRef>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogueDbContext).Assembly);
    }

}
