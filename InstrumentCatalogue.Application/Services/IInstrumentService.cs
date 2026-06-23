using InstrumentCatalogue.Application.DTOs.Instrument;
using System.Diagnostics.Metrics;

namespace InstrumentCatalogue.Application.Services;

public interface IInstrumentService
{
    Task<Guid> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default);
}
