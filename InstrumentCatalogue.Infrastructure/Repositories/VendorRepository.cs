using Dapper;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InstrumentCatalogue.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly CatalogueDbContext _dbContext;

    private readonly IDbConnection _connection;

    public VendorRepository(CatalogueDbContext dbContext, IDbConnection connection)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
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

    public async Task<Vendor?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: "SELECT vendor_id, name, short_code, is_active from vendors where vendor_id = @vendor_id ",
            parameters: new { vendor_id = vendorId },
            cancellationToken: cancellationToken
            );
        var vendor = await _connection.QuerySingleOrDefaultAsync<Vendor?>(command);
        return vendor;
    }

    public Task<VendorInterface?> GetVendorInterfaceByIdAsync(int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<VendorInterface>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<Vendor>> GetVendorsAsync(CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: "SELECT vendor_id, name, short_code, is_active from vendors;",
            parameters: null,
            cancellationToken: cancellationToken
            );
        var vendors =  await _connection.QueryAsync<Vendor>(command);
        return vendors.ToList();

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
