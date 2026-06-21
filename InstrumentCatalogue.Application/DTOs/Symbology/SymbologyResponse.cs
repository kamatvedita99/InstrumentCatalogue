namespace InstrumentCatalogue.Application.DTOs.Symbology;

public record SymbologyResponse(int SymbologyId, string TypeCode, string? Description, bool IsActive);
