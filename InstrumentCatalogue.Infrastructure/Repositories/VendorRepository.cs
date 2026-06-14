using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Infrastructure.Persistence;

namespace InstrumentCatalogue.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly CatalogueDbContext _dbContext;
    public VendorRepository(CatalogueDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task<int> CreateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vendor, nameof(vendor));

        await _dbContext.AddAsync(vendor, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vendor.VendorId;
    }

    public Task<int> CreateVendorInterfaceAsync(VendorInterface vendorInterface, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Vendor?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<VendorInterface?> GetVendorInterfaceByIdAsync(int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<VendorInterface>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<Vendor>> GetVendorsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateVendorInterfaceAsync(VendorInterface vendorInterface, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
