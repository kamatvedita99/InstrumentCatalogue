using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Filters;

namespace InstrumentCatalogue.Application.Services;

public interface IInstrumentService
{
    Task<InstrumentResponse> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default);

    Task<InstrumentResponse?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<PagedResult<InstrumentResponse>> GetAllAsync(PagedRequest<InstrumentFilter> pagedRequest, CancellationToken cancellationToken = default);

    Task<InstrumentResponse?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default);
}
