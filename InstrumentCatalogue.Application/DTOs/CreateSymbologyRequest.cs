using System.ComponentModel.DataAnnotations;

namespace InstrumentCatalogue.Application.DTOs;

public class CreateSymbologyRequest
{
    [Required]
    public string TypeCode { get; set; } = string.Empty;

    public string? Description { get; set; }
}
