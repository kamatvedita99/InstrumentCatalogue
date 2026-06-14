namespace InstrumentCatalogue.Application.DTOs;

public record SymbologyResponse(int SymbologyId, string TypeCode, string? Description, bool IsActive);
