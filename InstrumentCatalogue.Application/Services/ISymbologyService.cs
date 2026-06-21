using InstrumentCatalogue.Application.DTOs.Symbology;

namespace InstrumentCatalogue.Application.Services;

public interface ISymbologyService
{
    Task<SymbologyResponse> CreateSymbologyAsync(CreateSymbologyRequest request, CancellationToken cancellationToken = default);

    Task<ICollection<SymbologyResponse>> GetSymbologiesAsync(CancellationToken cancellationToken = default);

    Task<SymbologyResponse?> GetSymbologyByIdAsync(int symbologyId, CancellationToken cancellationToken = default);

    Task<SymbologyResponse?> UpdateSymbologyAsync(int symbologyId, UpdateSymbologyRequest request, CancellationToken cancellationToken = default);
}
