using InstrumentCatalogue.Application.DTOs.Instrument;

namespace InstrumentCatalogue.Application.Services;

public interface IInstrumentService
{
    Task<InstrumentResponse> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default);
}
