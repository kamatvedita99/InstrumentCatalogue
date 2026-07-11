using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;
using InstrumentCatalogue.Application.DTOs.SymbolXRef;
using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Filters;

namespace InstrumentCatalogue.Application.Services;

public interface IInstrumentService
{
    Task<InstrumentResponse> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default);

    Task<InstrumentResponse?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<PagedResult<InstrumentResponse>> GetAllAsync(PagedRequest<InstrumentFilter> pagedRequest, CancellationToken cancellationToken = default);

    Task<ResolvedSymbol?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default);

    Task<SymbolXRefResponse?>CreateSymbolAsync(Guid instrumentId, CreateInstrumentSymbolRequest request, CancellationToken cancellationToken = default);

    Task<ICollection<InstrumentStatusHistoryResponse>> GetInstrumentStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken= default);
}
