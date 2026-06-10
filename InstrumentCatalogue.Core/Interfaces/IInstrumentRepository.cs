using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Core.ReadModels;

namespace InstrumentCatalogue.Core.Interfaces;

public interface IInstrumentRepository
{
    Task<PagedResult<Instrument>> GetAsync(InstrumentFilter filter,
                                           CancellationToken cancellationToken=default);

    Task<Instrument?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken=default);

    Task<ICollection<SymbolXRef>> GetSymbolsAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<ICollection<InstrumentStatusHistory>> GetStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<Instrument?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default);

    Task<PagedResult<InstrumentSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<IEnumerable<Instrument>> GetSnapshotAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Instrument instrument, CancellationToken cancellationToken = default);

    Task UpdateAsync(Instrument instrument, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid instrumentId, DateOnly effectiveDate, InstrumentStatus instrumentStatus, string? notes, CancellationToken cancellationToken = default);

}
