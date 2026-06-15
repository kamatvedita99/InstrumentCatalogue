using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Services;

public interface ISymbologyService
{
    Task<SymbologyResponse> CreateSymbologyAsync(CreateSymbologyRequest request, CancellationToken cancellationToken);

    Task<ICollection<SymbologyResponse>> GetSymbologiesAsync(CancellationToken cancellationToken);
}
