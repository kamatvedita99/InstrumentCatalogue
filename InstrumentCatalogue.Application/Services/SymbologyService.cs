using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Interfaces;

namespace InstrumentCatalogue.Application.Services;

public class SymbologyService : ISymbologyService
{
    private readonly ISymbologyRepository _symbologyRepository;

    public SymbologyService(ISymbologyRepository symbologyRepository)
    {
        _symbologyRepository = symbologyRepository;
    }

    public async Task<SymbologyResponse> CreateSymbologyAsync(CreateSymbologyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var symbology = SymbologyMapper.ToDomain(request);
        await _symbologyRepository.CreateSymbologyAsync(symbology, cancellationToken);
        return SymbologyMapper.ToResponse(symbology);
        
    }

    public async Task<ICollection<SymbologyResponse>> GetSymbologiesAsync(CancellationToken cancellationToken = default)
    {
        var symbologies = await _symbologyRepository.GetSymbologiesAsync(cancellationToken);
        return symbologies.Select(SymbologyMapper.ToResponse).ToList();
    }
}
