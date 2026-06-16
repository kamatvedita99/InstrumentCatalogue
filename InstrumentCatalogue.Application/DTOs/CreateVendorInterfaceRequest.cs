using System.ComponentModel.DataAnnotations;

namespace InstrumentCatalogue.Application.DTOs;

public class CreateVendorInterfaceRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Protocol { get; set; }

    public string? Description { get; set; }

}
