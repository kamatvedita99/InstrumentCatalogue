using InstrumentCatalogue.Application.DTOs.VendorInterfaceSymbol;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class VendorInterfaceSymbolMapper
{
    public static VendorInterfaceSymbolResponse ToResponse(VendorInterfaceSymbolXRef vendorInterfaceSymbolXRef)
    {
        ArgumentNullException.ThrowIfNull(vendorInterfaceSymbolXRef);

        return new VendorInterfaceSymbolResponse
        {
            VendorInterfaceId = vendorInterfaceSymbolXRef.VendorInterfaceId,
            VendorInterfaceSymbolXRefId = vendorInterfaceSymbolXRef.VendorInterfaceSymbolXRefId,
            SymbolXRefId = vendorInterfaceSymbolXRef.SymbolXRefId,
            IsActive = vendorInterfaceSymbolXRef.IsActive,
            ReceivedAtUtc = vendorInterfaceSymbolXRef.ReceivedAtUtc,
        };
    }
}
