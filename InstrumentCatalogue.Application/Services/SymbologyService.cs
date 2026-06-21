using InstrumentCatalogue.Application.DTOs.Symbology;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;

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

    public async Task<SymbologyResponse?> GetSymbologyByIdAsync(int symbologyId, CancellationToken cancellationToken = default)
    {
       var symbology = await _symbologyRepository.GetSymbologyByIdAsync(symbologyId, cancellationToken);
        
        if(symbology == null)
            throw new NotFoundException<int>(nameof(Symbology), symbologyId);

        return SymbologyMapper.ToResponse(symbology);
    }

    public async Task<SymbologyResponse?> UpdateSymbologyAsync(int symbologyId, UpdateSymbologyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var symbology = await _symbologyRepository.GetSymbologyByIdAsync(symbologyId, cancellationToken);

        if(symbology == null)
            throw new NotFoundException<int>(nameof(Symbology), symbologyId);

        if(request.IsActive.HasValue)
            symbology.IsActive = request.IsActive.Value;

        if(!string.IsNullOrWhiteSpace(request.Description))
            symbology.Description = request.Description;

        symbology.StampUpdated();

        await _symbologyRepository.UpdateSymbologyAsync(symbology, cancellationToken);
        return SymbologyMapper.ToResponse(symbology);
    }
}
