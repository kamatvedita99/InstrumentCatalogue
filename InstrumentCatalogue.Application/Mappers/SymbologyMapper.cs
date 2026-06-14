using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class SymbologyMapper
{
    public static Symbology ToDomain(CreateSymbologyRequest symbologyRequest)
    {
        ArgumentNullException.ThrowIfNull(nameof(symbologyRequest));

        return new Symbology()
        {
            SymbologyId = 0,
            TypeCode = symbologyRequest.TypeCode,
            Description = symbologyRequest.Description ?? string.Empty,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedAtUtc = DateTime.UtcNow

        };
    }

    public static SymbologyResponse ToResponse(Symbology symbology)
    {
        ArgumentNullException.ThrowIfNull(nameof(symbology));

        return new SymbologyResponse(
            symbology.SymbologyId,
            symbology.TypeCode,
            symbology.Description,
            symbology.IsActive);

    }
}
