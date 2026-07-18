using System.ComponentModel.DataAnnotations;

namespace InstrumentCatalogue.Application.DTOs.VendorInterfaceSymbol;

public class CreateVendorInterfaceSymbolRequest
{
        [Required]
        public int VendorInterfaceId { get; set; }

        [Required]
        public DateTime ReceivedAtUtc { get; set; }
    
}
