using Dapper;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Core.ReadModels;
using InstrumentCatalogue.Infrastructure.Persistence;
using System.Data;

namespace InstrumentCatalogue.Infrastructure.Repositories;

public class InstrumentRepository : IInstrumentRepository
{
    private readonly CatalogueDbContext _dbContext;

    private readonly IDbConnection _dbConnection;
    public InstrumentRepository(CatalogueDbContext dbContext, IDbConnection dbConnection)
    {
        _dbContext = dbContext;
        _dbConnection = dbConnection;
    }
    public async Task<Guid> CreateAsync(Instrument instrument, int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
           await _dbContext.AddAsync(instrument, cancellationToken);
           await _dbContext.SaveChangesAsync(cancellationToken);

            var vendorInterfaceSymbolXRefs = instrument.Symbols.Select(symbol => new VendorInterfaceSymbolXRef
            {
                SymbolXRefId = symbol.SymbolXRefId,
                VendorInterfaceId = vendorInterfaceId,
                ReceivedAtUtc = DateTime.UtcNow
            }).ToList();

            await _dbContext.AddRangeAsync(vendorInterfaceSymbolXRefs, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return instrument.InstrumentId;

        }

        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;

        }


    }

    public Task<PagedResult<Instrument>> GetAsync(InstrumentFilter filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Instrument?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT instrument_id, name, type, exchange, currency, country, listed_date,
               created_at_utc, last_updated_at_utc
        FROM instruments
        WHERE instrument_id = @instrument_id;
 
        SELECT instrument_id, sector, industry, shares_outstanding, lot_size, par_value,
               created_at_utc, last_updated_at_utc
        FROM equity_ref_data
        WHERE instrument_id = @instrument_id;
 
        SELECT instrument_id, face_value, coupon_rate, coupon_frequency, issuer, issue_date,
               maturity_date, credit_rating, bond_type, bond_structure, duration,
               created_at_utc, last_updated_at_utc
        FROM bond_ref_data
        WHERE instrument_id = @instrument_id;
 
        SELECT instrument_id, fund_manager, underlying_index, replication_type,
               distribution_frequency, inception_date, expense_ratio,
               created_at_utc, last_updated_at_utc
        FROM etf_ref_data
        WHERE instrument_id = @instrument_id;
 
        SELECT instrument_status_history_id, instrument_id, valid_from, valid_to,
               effective_date, notes, instrument_status, created_at_utc, last_updated_at_utc
        FROM instrument_status_history
        WHERE instrument_id = @instrument_id;
 
        SELECT symbol_x_ref_id, symbology_id, symbol, instrument_id, is_primary,
               valid_from, valid_to, created_at_utc, last_updated_at_utc
        FROM symbol_x_ref
        WHERE instrument_id = @instrument_id;
 
        SELECT visxr.vendor_interface_symbol_x_ref_id, visxr.vendor_interface_id,
               visxr.symbol_x_ref_id, visxr.received_at_utc, visxr.is_active,
               visxr.created_at_utc, visxr.last_updated_at_utc
        FROM vendor_interface_symbol_x_ref visxr
        INNER JOIN symbol_x_ref sxr ON sxr.symbol_x_ref_id = visxr.symbol_x_ref_id
        WHERE sxr.instrument_id = @instrument_id;
 
        SELECT symbology_id, type_code, description, is_active, created_at_utc, last_updated_at_utc
        FROM symbologies;";

        var command = new CommandDefinition(sql, new { instrument_id = instrumentId }, cancellationToken:cancellationToken);

        using var multiResultSet = await _dbConnection.QueryMultipleAsync(command);

        var instrument = await multiResultSet.ReadSingleOrDefaultAsync<Instrument>();
        if (instrument is null)
            return null;

        
        instrument.EquityRefData = await multiResultSet.ReadFirstOrDefaultAsync<EquityRefData>();
        instrument.BondRefData = await multiResultSet.ReadFirstOrDefaultAsync<BondRefData>();
        instrument.EtfRefData = await multiResultSet.ReadFirstOrDefaultAsync<EtfRefData>();

        instrument.StatusHistory = (await multiResultSet.ReadAsync<InstrumentStatusHistory>()).ToList();

        var symbols = (await multiResultSet.ReadAsync<SymbolXRef>()).ToList();

        var vendorXRefsBySymbol = (await multiResultSet.ReadAsync<VendorInterfaceSymbolXRef>())
        .GroupBy(v => v.SymbolXRefId)
        .ToDictionary(g => g.Key, g => g.ToList());

        var symbologiesById = (await multiResultSet.ReadAsync<Symbology>())
        .ToDictionary(s => s.SymbologyId);

        foreach (var symbol in symbols)
        {
            symbol.VendorInterfaceSymbols = vendorXRefsBySymbol.GetValueOrDefault(symbol.SymbolXRefId)
                                             ?? new List<VendorInterfaceSymbolXRef>();

            if (symbologiesById.TryGetValue(symbol.SymbologyId, out var symbology))
                symbol.Symbology = symbology;
        }

        instrument.Symbols = symbols;

        return instrument;


    }

    public Task<IEnumerable<Instrument>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<InstrumentStatusHistory>> GetStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<SymbolXRef>> GetSymbolsAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Instrument?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<InstrumentSearchResult>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Instrument instrument, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateStatusAsync(Guid instrumentId, DateOnly effectiveDate, InstrumentStatus instrumentStatus, string? notes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
