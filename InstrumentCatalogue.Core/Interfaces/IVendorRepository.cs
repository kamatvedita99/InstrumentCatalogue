using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Core.Interfaces;

public interface IVendorRepository
{
    Task<int> CreateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);

    Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);

    Task<Vendor?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default);

    Task<ICollection<Vendor>> GetVendorsAsync(CancellationToken cancellationToken = default);

    Task<int> CreateVendorInterfaceAsync(VendorInterface vendorInterface, CancellationToken cancellationToken = default);

    Task UpdateVendorInterfaceAsync(VendorInterface vendorInterface, CancellationToken cancellationToken = default);

    Task<VendorInterface?> GetVendorInterfaceByIdAsync(int vendorInterfaceId, CancellationToken cancellationToken = default);

    Task<ICollection<VendorInterface>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default);


}
