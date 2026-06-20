using Dapper;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Infrastructure.Persistence;
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
        ArgumentNullException.ThrowIfNull(vendor);

        await _dbContext.AddAsync(vendor, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vendor.VendorId;
    }

    public async Task<int> CreateVendorInterfaceAsync(VendorInterface vendorInterface, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vendorInterface);
        await _dbContext.AddAsync(vendorInterface, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vendorInterface.VendorInterfaceId;
    }

    public async Task<Vendor?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: "SELECT vendor_id, name, short_code, is_active, created_at_utc, last_updated_at_utc from vendors where vendor_id = @vendor_id ",
            parameters: new { vendor_id = vendorId },
            cancellationToken: cancellationToken
            );
        var vendor = await _connection.QuerySingleOrDefaultAsync<Vendor?>(command);
        return vendor;
    }

    public async Task<VendorInterface?> GetVendorInterfaceByIdAsync(int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(

            commandText: "SELECT vendor_interface_id, vendor_id, name, description, protocol, is_active, created_at_utc, last_updated_at_utc from vendor_interfaces where vendor_interface_id = @vendor_interface_id;", 
            parameters: new { vendor_interface_id = vendorInterfaceId },
            cancellationToken: cancellationToken
            );

       return await  _connection.QueryFirstOrDefaultAsync<VendorInterface?>(command);
    }

    public async Task<VendorInterface?> GetVendorInterfaceByIdAsync(int vendorId, int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(

            commandText: "SELECT vendor_interface_id, vendor_id, name, description, protocol, is_active, created_at_utc, last_updated_at_utc " +
            "from vendor_interfaces where vendor_id = @vendor_id AND vendor_interface_id = @vendor_interface_id;",
            parameters: new { vendor_id = vendorId, vendor_interface_id = vendorInterfaceId },
            cancellationToken: cancellationToken
            );

        return await _connection.QueryFirstOrDefaultAsync<VendorInterface?>(command);
    }

    public async Task<ICollection<VendorInterface>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(

            commandText: "SELECT vendor_interface_id, vendor_id, name, description, protocol, is_active, created_at_utc, last_updated_at_utc from vendor_interfaces where vendor_id = @vendor_id",
            parameters: new { vendor_id = vendorId },
            cancellationToken: cancellationToken
            );

        var vendorInterfaces =  await _connection.QueryAsync<VendorInterface>(command);
        return vendorInterfaces.ToList();
    }

    public async Task<ICollection<Vendor>> GetVendorsAsync(CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: "SELECT vendor_id, name, short_code, is_active, created_at_utc, last_updated_at_utc from vendors;",
            parameters: null,
            cancellationToken: cancellationToken
            );
        var vendors =  await _connection.QueryAsync<Vendor>(command);
        return vendors.ToList();

    }

    public async Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vendor);

        _dbContext.Update(vendor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
    }

    public async Task UpdateVendorInterfaceAsync(VendorInterface vendorInterface, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vendorInterface);
        
        _dbContext.Update(vendorInterface);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
