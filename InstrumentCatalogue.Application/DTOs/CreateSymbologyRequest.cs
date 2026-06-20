namespace InstrumentCatalogue.Application.DTOs;

public class CreateSymbologyRequest
{
    public string TypeCode { get; set; } = string.Empty;

    public string? Description { get; set; }
}
