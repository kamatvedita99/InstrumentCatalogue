using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Infrastructure.Persistence;

namespace InstrumentCatalogue.Infrastructure.Repositories;

public class SymbologyRepository : ISymbologyRepository
{
    private readonly CatalogueDbContext _dbContext;
    public SymbologyRepository(CatalogueDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public Task<Guid> CreateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default)
    {
       ArgumentNullException.ThrowIfNull(symbology);
       await _dbContext.AddAsync(symbology, cancellationToken);
       await _dbContext.SaveChangesAsync(cancellationToken);
       return symbology.SymbologyId;
    }

    public Task<Guid> CreateVendorInterfaceSymbolAsync(VendorInterfaceSymbolXRef xref, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SymbolXRef?> GetSymbolByIdAsync(Guid symbolXRefId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<Symbology>> GetSymbologiesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Symbology?> GetSymbologyByIdAsync(int symbologyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<SymbolXRef>> GetSymbolsAsync(int symbologyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSymbolValidToAsync(Guid symbolXRefId, DateOnly validTo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateVendorInterfaceSymbolAsync(Guid vendorInterfaceSymbolXRefId, bool isActive, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
