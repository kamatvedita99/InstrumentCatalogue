using Dapper;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Infrastructure.Persistence;
using System.Data;

namespace InstrumentCatalogue.Infrastructure.Repositories;

public class SymbologyRepository : ISymbologyRepository
{
    private readonly CatalogueDbContext _dbContext;

    private readonly IDbConnection _dbConnection;
    public SymbologyRepository(CatalogueDbContext dbContext, IDbConnection dbConnection)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }
    public Task<Guid> CreateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default)
    {
       ArgumentNullException.ThrowIfNull(symbology);
       await _dbContext.AddAsync(symbology, cancellationToken);
       await _dbContext.SaveChangesAsync(cancellationToken);
       return symbology.SymbologyId;
    }

    public Task<Guid> CreateVendorInterfaceSymbolAsync(VendorInterfaceSymbolXRef xref, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SymbolXRef?> GetSymbolByIdAsync(Guid symbolXRefId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<Symbology>> GetSymbologiesAsync(CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(

            commandText: "SELECT symbology_id, type_code, description, is_active, created_at_utc, last_updated_at_utc from symbologies;",
            parameters: null,
            cancellationToken: cancellationToken

            );
        var symbologies = await _dbConnection.QueryAsync<Symbology>(command);
        return symbologies.ToList();
    }

    public async Task<ICollection<Symbology>> GetSymbologiesByTypeCodeAsync(ICollection<string> typeCodes, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(

            commandText: "SELECT symbology_id, type_code, description, is_active, created_at_utc, last_updated_at_utc from symbologies WHERE type_code = ANY(@type_codes);",
            parameters: new { type_codes = typeCodes},
            cancellationToken: cancellationToken

            );
        var symbologies = await _dbConnection.QueryAsync<Symbology>(command);
        return symbologies.ToList();

    }

    public async Task<Symbology?> GetSymbologyByIdAsync(int symbologyId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: "SELECT symbology_id, type_code, description, is_active, created_at_utc, last_updated_at_utc from symbologies where symbology_id = @symbology_id",
            parameters: new { symbology_id = symbologyId },
            cancellationToken: cancellationToken

            );
        return await _dbConnection.QuerySingleOrDefaultAsync<Symbology?>(command);
    }

    public Task<PagedResult<SymbolXRef>> GetSymbolsAsync(int symbologyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(symbology);

        _dbContext.Update(symbology);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateSymbolValidToAsync(Guid symbolXRefId, DateOnly validTo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateVendorInterfaceSymbolAsync(Guid vendorInterfaceSymbolXRefId, bool isActive, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
