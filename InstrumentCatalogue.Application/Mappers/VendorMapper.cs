using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class VendorMapper
{
    public static VendorResponse ToResponse(Vendor vendor)
    {
        ArgumentNullException.ThrowIfNull(vendor);
        return new VendorResponse(vendor.VendorId, vendor.Name, vendor.ShortCode, vendor.IsActive);

    }

    public static Vendor ToDomain(CreateVendorRequest request)
    {

        ArgumentNullException.ThrowIfNull(request);
        return new Vendor
        {
            VendorId = 0,
            Name = request.Name,
            ShortCode = request.ShortCode,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedAtUtc = DateTime.UtcNow
        };
    }
}
