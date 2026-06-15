using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Services;

public interface IVendorService
{
    Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken);

    Task<VendorResponse?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken);

    Task<ICollection<VendorResponse>> GetVendorsAsync(CancellationToken cancellationToken);

    Task<VendorResponse?> UpdateVendorAsync(int vendorId, UpdateVendorRequest request, CancellationToken cancellationToken);

}
