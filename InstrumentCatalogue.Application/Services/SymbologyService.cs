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

    public async Task<SymbologyResponse> CreateSymbologyAsync(CreateSymbologyRequest request, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(nameof(request));
        var symbology = SymbologyMapper.ToDomain(request);
        await _symbologyRepository.CreateSymbologyAsync(symbology, cancellationToken);
        return SymbologyMapper.ToResponse(symbology);
        
    }
}
