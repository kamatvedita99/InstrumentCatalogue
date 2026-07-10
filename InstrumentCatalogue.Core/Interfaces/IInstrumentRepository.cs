using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Core.ReadModels;
using System.Threading;

namespace InstrumentCatalogue.Core.Interfaces;

public interface IInstrumentRepository
{
    Task<PagedResult<Instrument>> GetAsync(PagedRequest<InstrumentFilter> pagedRequest,
                                           CancellationToken cancellationToken=default);

    Task<Instrument?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken=default);

    Task<ICollection<SymbolXRef>> GetSymbolsAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<ICollection<InstrumentStatusHistory>> GetStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<ResolvedSymbol?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default);

    Task<PagedResult<InstrumentSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<IEnumerable<Instrument>> GetSnapshotAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Instrument instrument, ICollection<VendorInterfaceSymbolXRef> vendorInterfaceSymbolXRefs, CancellationToken cancellationToken = default);

    Task UpdateAsync(Instrument instrument, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid instrumentId, DateOnly effectiveDate, InstrumentStatus instrumentStatus, string? notes, CancellationToken cancellationToken = default);

    Task<Guid> CreateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default);

    Task UpdateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default);

    Task UpdateSymbolValidToAsync(Guid symbolXRefId, DateOnly validTo, CancellationToken cancellationToken = default);

    Task<SymbolXRef?> GetSymbolByIdAsync(Guid symbolXRefId, CancellationToken cancellationToken = default);

    Task<PagedResult<SymbolXRef>> GetSymbolsAsync(int symbologyId, CancellationToken cancellationToken = default);

    Task<Guid> CreateVendorInterfaceSymbolAsync(VendorInterfaceSymbolXRef xref, CancellationToken cancellationToken = default);

    Task UpdateVendorInterfaceSymbolAsync(Guid vendorInterfaceSymbolXRefId, bool isActive, CancellationToken cancellationToken = default);

    Task<SymbolXRef?> GetActiveSymbolAsync(Guid instrumentId, int symbologyId, CancellationToken cancellationToken = default);

}
