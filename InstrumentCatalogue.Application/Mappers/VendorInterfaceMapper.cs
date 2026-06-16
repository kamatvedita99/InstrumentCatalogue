using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class VendorInterfaceMapper
{
    public static VendorInterface ToDomain(int vendorId, CreateVendorInterfaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new VendorInterface
        {
            VendorInterfaceId = 0,
            VendorId = vendorId,
            Name = request.Name,
            Description = request.Description,
            Protocol = request.Protocol,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedAtUtc = DateTime.UtcNow,

        };

    }

    public static VendorInterfaceResponse ToResponse(VendorInterface vendorInterface) 
    {

        ArgumentNullException.ThrowIfNull(vendorInterface);

        return new VendorInterfaceResponse(vendorInterface.VendorInterfaceId, vendorInterface.VendorId, vendorInterface.Name, vendorInterface.Description, vendorInterface.Protocol, vendorInterface.IsActive);

        
    }
}
