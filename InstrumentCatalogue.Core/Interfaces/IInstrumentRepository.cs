using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Core.Interfaces;

public interface IInstrumentRepository
{
    Task<PagedResult<Instrument>> GetAsync(//InstrumentFilter filter,
                                           CancellationToken cancellationToken=default);//pass instrument filter object as input

    Task<Instrument?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken=default);

    Task<ICollection<SymbolXRef>> GetSymbolsAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<ICollection<InstrumentStatusHistory>> GetStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<Instrument?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default);

    Task<PagedResult<Instrument>> SearchAsync(string searchTerm, CancellationToken cancellation);

    Task<IEnumerable<Instrument>> GetSnapshotAsync(CancellationToken cancellationToken = default);

}
