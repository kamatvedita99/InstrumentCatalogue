using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Services;

public interface IVendorService
{
    Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken);

    Task<VendorResponse?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken);

    Task<ICollection<VendorResponse>> GetVendorsAsync(CancellationToken cancellationToken);

}
