using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Core.ReadModels;
using InstrumentCatalogue.Infrastructure.Persistence;

namespace InstrumentCatalogue.Infrastructure.Repositories;

public class InstrumentRepository : IInstrumentRepository
{
    private readonly CatalogueDbContext _dbContext;
    public InstrumentRepository(CatalogueDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Guid> CreateAsync(Instrument instrument, int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
           await _dbContext.AddAsync(instrument, cancellationToken);
           await _dbContext.SaveChangesAsync(cancellationToken);

            var vendorInterfaceSymbolXRefs = instrument.Symbols.Select(symbol => new VendorInterfaceSymbolXRef
            {
                SymbolXRefId = symbol.SymbolXRefId,
                VendorInterfaceId = vendorInterfaceId,
                ReceivedAtUtc = DateTime.UtcNow
            }).ToList();

            await _dbContext.AddRangeAsync(vendorInterfaceSymbolXRefs, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return instrument.InstrumentId;

        }

        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;

        }


    }

    public Task<PagedResult<Instrument>> GetAsync(InstrumentFilter filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Instrument?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Instrument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<InstrumentStatusHistory>> GetStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<SymbolXRef>> GetSymbolsAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Instrument?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<InstrumentSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Instrument instrument, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateStatusAsync(Guid instrumentId, DateOnly effectiveDate, InstrumentStatus instrumentStatus, string? notes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
