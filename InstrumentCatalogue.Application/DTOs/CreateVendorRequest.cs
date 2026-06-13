using System.ComponentModel.DataAnnotations;

namespace InstrumentCatalogue.Application.DTOs;

public class CreateVendorRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string ShortCode { get; set; } = string.Empty;

}
