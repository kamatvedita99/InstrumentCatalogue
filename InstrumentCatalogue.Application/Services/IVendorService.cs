using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Services;

public interface IVendorService
{
    Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken = default);

    Task<VendorResponse?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default);

    Task<ICollection<VendorResponse>> GetVendorsAsync(CancellationToken cancellationToken = default);

    Task<VendorResponse?> UpdateVendorAsync(int vendorId, UpdateVendorRequest request, CancellationToken cancellationToken = default);

    Task<VendorInterfaceResponse> CreateVendorInterfaceAsync(int vendorId, CreateVendorInterfaceRequest request, CancellationToken cancellationToken = default);

    Task<VendorInterfaceResponse?> GetVendorInterfaceByIdAsync(int vendorInterfaceId, CancellationToken cancellationToken = default);

    Task<ICollection<VendorInterfaceResponse>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default);

    Task<VendorInterfaceResponse?> UpdateVendorInterfaceAsync(CancellationToken cancellationToken= default); 

}
