using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class SymbologyMapper
{
    public static Symbology ToDomain(CreateSymbologyRequest symbologyRequest)
    {
        ArgumentNullException.ThrowIfNull(symbologyRequest);

        var symbology =  new Symbology()
        {
            SymbologyId = 0,
            TypeCode = symbologyRequest.TypeCode,
            Description = symbologyRequest.Description ?? string.Empty,
            IsActive = true
        };

        return symbology.StampCreated();

    }

    public static SymbologyResponse ToResponse(Symbology symbology)
    {
        ArgumentNullException.ThrowIfNull(symbology);

        return new SymbologyResponse(
            symbology.SymbologyId,
            symbology.TypeCode,
            symbology.Description,
            symbology.IsActive);

    }
}
