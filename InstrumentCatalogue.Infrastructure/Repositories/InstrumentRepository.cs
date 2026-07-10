using Dapper;
using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Constants;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Helpers;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Core.ReadModels;
using InstrumentCatalogue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Guid> CreateAsync(Instrument instrument, ICollection<VendorInterfaceSymbolXRef> vendorInterfaceSymbolXRefs, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
           await _dbContext.AddAsync(instrument, cancellationToken);
           await _dbContext.SaveChangesAsync(cancellationToken);

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

    public async Task<Guid> CreateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default)
    {
       
            await _dbContext.AddAsync<SymbolXRef>(symbolXRef, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
        
            return symbolXRef.SymbolXRefId;
    }

    public Task<Guid> CreateVendorInterfaceSymbolAsync(VendorInterfaceSymbolXRef xref, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<Instrument>> GetAsync(PagedRequest<InstrumentFilter> pagedRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pagedRequest);

        var filter = pagedRequest.Filter;
        var conditions = new List<string> { "1=1" };
        var parameters = new DynamicParameters();
        var joins = new List<string>();

        if (filter.Type.HasValue)
        {
            conditions.Add("i.type = @type");
            parameters.Add("type", filter.Type.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(filter.Currency))
        {
            conditions.Add("i.currency = @currency");
            parameters.Add("currency", filter.Currency);
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            conditions.Add("i.country = @country");
            parameters.Add("country", filter.Country);
        }

        if (!string.IsNullOrWhiteSpace(filter.Exchange))
        {
            conditions.Add("i.exchange = @exchange");
            parameters.Add("exchange", filter.Exchange);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            conditions.Add("i.name ILIKE @name");
            parameters.Add("name", $"%{filter.Name}%");
        }

        if (filter.Status.HasValue)
        {
            conditions.Add("ish.instrument_status = @status");
            parameters.Add("status", filter.Status.Value.ToString());
        }

        if (filter.ListedDateFrom.HasValue)
        {
            conditions.Add("i.listed_date >= @listed_date_from");
            parameters.Add("listed_date_from", filter.ListedDateFrom.Value.ToString());
        }

        if (filter.ListedDateTo.HasValue)
        {
            conditions.Add("i.listed_date <= @listed_date_to");
            parameters.Add("listed_date_to", filter.ListedDateTo.Value.ToString());
        }

        if (filter.BondFilter != null)
        {
            joins.Add("INNER JOIN bond_ref_data brd ON brd.instrument_id = i.instrument_id");

            if (filter.BondFilter.BondType.HasValue)
            {
                conditions.Add("brd.bond_type = @bond_type");
                parameters.Add("bond_type", filter.BondFilter.BondType.Value.ToString());
            }
            if (filter.BondFilter.BondStructure.HasValue)
            {
                conditions.Add("brd.bond_structure = @bond_structure");
                parameters.Add("bond_structure", filter.BondFilter.BondStructure.Value.ToString());
            }
            if (filter.BondFilter.MaturityBefore.HasValue)
            {
                conditions.Add("brd.maturity_date <= @maturity_before");
                parameters.Add("maturity_before", filter.BondFilter.MaturityBefore.Value);
            }
            if (filter.BondFilter.MaturityAfter.HasValue)
            {
                conditions.Add("brd.maturity_date >= @maturity_after");
                parameters.Add("maturity_after", filter.BondFilter.MaturityAfter.Value);
            }
            if (filter.BondFilter.IssueDate.HasValue)
            {
                conditions.Add("brd.issue_date = @issue_date");
                parameters.Add("issue_date", filter.BondFilter.IssueDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.BondFilter.CreditRating))
            {
                conditions.Add("brd.credit_rating = @credit_rating");
                parameters.Add("credit_rating", filter.BondFilter.CreditRating);
            }
            if (!string.IsNullOrWhiteSpace(filter.BondFilter.Issuer))
            {
                conditions.Add("brd.issuer = @issuer");
                parameters.Add("issuer", filter.BondFilter.Issuer);
            }
        }

        if (filter.EquityFilter != null)
        {
            joins.Add("INNER JOIN equity_ref_data erd ON erd.instrument_id = i.instrument_id");

            if (!string.IsNullOrWhiteSpace(filter.EquityFilter.Sector))
            {
                conditions.Add("erd.sector = @sector");
                parameters.Add("sector", filter.EquityFilter.Sector);
            }
            if (!string.IsNullOrWhiteSpace(filter.EquityFilter.Industry))
            {
                conditions.Add("erd.industry = @industry");
                parameters.Add("industry", filter.EquityFilter.Industry);
            }
        }

        if (filter.EtfFilter != null)
        {
            joins.Add("INNER JOIN etf_ref_data etfrd ON etfrd.instrument_id = i.instrument_id");

            if (!string.IsNullOrWhiteSpace(filter.EtfFilter.UnderlyingIndex))
            {
                conditions.Add("etfrd.underlying_index = @underlying_index");
                parameters.Add("underlying_index", filter.EtfFilter.UnderlyingIndex);
            }
            if (filter.EtfFilter.ReplicationType.HasValue)
            {
                conditions.Add("etfrd.replication_type = @replication_type");
                parameters.Add("replication_type", filter.EtfFilter.ReplicationType.Value.ToString());
            }
            if (filter.EtfFilter.DistributionFrequency.HasValue)
            {
                conditions.Add("etfrd.distribution_frequency = @distribution_frequency");
                parameters.Add("distribution_frequency", filter.EtfFilter.DistributionFrequency.Value.ToString());
            }
            if (filter.EtfFilter.InceptionDate.HasValue)
            {
                conditions.Add("etfrd.inception_date = @inception_date");
                parameters.Add("inception_date", filter.EtfFilter.InceptionDate.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(pagedRequest.Cursor))
        {
            var cursorPayload = CursorHelper.Decode<InstrumentCursorPayload>(pagedRequest.Cursor);
            conditions.Add("(i.created_at_utc < @cursor_created_at OR (i.created_at_utc = @cursor_created_at AND i.instrument_id < @cursor_instrument_id))");
            parameters.Add("cursor_created_at", cursorPayload.CreatedAtUtc);
            parameters.Add("cursor_instrument_id", cursorPayload.InstrumentId);
        }
        var limit = "LIMIT @limit";
        parameters.Add("limit", pagedRequest.PageSize + 1);

        var sql = $@"
        SELECT i.instrument_id, i.name, i.type, i.exchange, i.currency, i.country, 
               i.listed_date, i.created_at_utc, i.last_updated_at_utc
        FROM instruments i
        INNER JOIN instrument_status_history ish 
            ON ish.instrument_id = i.instrument_id 
            AND ish.valid_to = '9999-12-31'
        {string.Join(" ", joins)}
        WHERE {string.Join(" AND ", conditions)}
        ORDER BY i.created_at_utc DESC, i.instrument_id DESC
        {limit}
    ";

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        var instruments = (await _dbConnection.QueryAsync<Instrument>(command)).ToList();
        string? nextCursor = null;
        if (instruments.Count > pagedRequest.PageSize)
        {
            instruments.RemoveAt(instruments.Count - 1);
            var nextCursorPayload = new InstrumentCursorPayload(instruments[instruments.Count-1].CreatedAtUtc, instruments[instruments.Count-1].InstrumentId);
            nextCursor = CursorHelper.Encode(nextCursorPayload);
            
        }

        var instrumentIds = instruments.Select(i => i.InstrumentId).ToArray();

        if (instrumentIds.Length > 0)
        {
            const string refDataSql = @"
        SELECT instrument_id, sector, industry, shares_outstanding, lot_size, par_value, created_at_utc, last_updated_at_utc
        FROM equity_ref_data WHERE instrument_id = ANY(@ids);

        SELECT instrument_id, face_value, coupon_rate, coupon_frequency, issuer, issue_date, maturity_date,
               credit_rating, bond_type, bond_structure, duration, created_at_utc, last_updated_at_utc
        FROM bond_ref_data WHERE instrument_id = ANY(@ids);

        SELECT instrument_id, fund_manager, underlying_index, replication_type, distribution_frequency,
               inception_date, expense_ratio, created_at_utc, last_updated_at_utc
        FROM etf_ref_data WHERE instrument_id = ANY(@ids);

        SELECT instrument_status_history_id, instrument_id, valid_from, valid_to, effective_date,
               notes, instrument_status, created_at_utc, last_updated_at_utc
        FROM instrument_status_history
        WHERE instrument_id = ANY(@ids) AND valid_to = '" + TemporalDefaults.CurrentSentinelSql + @"';
    ";

            using var multi = await _dbConnection.QueryMultipleAsync(
                new CommandDefinition(refDataSql, new { ids = instrumentIds }, cancellationToken: cancellationToken));

            var equityById = (await multi.ReadAsync<EquityRefData>()).ToDictionary(x => x.InstrumentId);
            var bondById = (await multi.ReadAsync<BondRefData>()).ToDictionary(x => x.InstrumentId);
            var etfById = (await multi.ReadAsync<EtfRefData>()).ToDictionary(x => x.InstrumentId);
            var statusById = (await multi.ReadAsync<InstrumentStatusHistory>()).ToDictionary(x => x.InstrumentId);

            foreach (var instrument in instruments)
            {
                instrument.EquityRefData = equityById.GetValueOrDefault(instrument.InstrumentId);
                instrument.BondRefData = bondById.GetValueOrDefault(instrument.InstrumentId);
                instrument.EtfRefData = etfById.GetValueOrDefault(instrument.InstrumentId);

                if (statusById.TryGetValue(instrument.InstrumentId, out var status))
                    instrument.StatusHistory = new List<InstrumentStatusHistory> { status };
            }
        }


        return new PagedResult<Instrument> { Items = instruments, NextCursor = nextCursor };
        
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

        
        instrument.EquityRefData = await multiResultSet.ReadSingleOrDefaultAsync<EquityRefData>();
        instrument.BondRefData = await multiResultSet.ReadSingleOrDefaultAsync<BondRefData>();
        instrument.EtfRefData = await multiResultSet.ReadSingleOrDefaultAsync<EtfRefData>();

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

    public Task<SymbolXRef?> GetSymbolByIdAsync(Guid symbolXRefId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<SymbolXRef>> GetSymbolsAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException();
    }

    public async Task<SymbolXRef?> GetActiveSymbolAsync(Guid instrumentId, int symbologyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SymbolXRefs
            .FirstOrDefaultAsync(s => s.InstrumentId == instrumentId
        && s.SymbologyId == symbologyId
        && s.ValidTo == DateOnly.Parse(TemporalDefaults.CurrentSentinelSql), cancellationToken); ;

    }

    public Task<PagedResult<SymbolXRef>> GetSymbolsAsync(int symbologyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ResolvedSymbol?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbology);

        var sql = "SELECT i.instrument_id, i.type, i.name " +
                "FROM instruments i " +
                "INNER JOIN symbol_x_ref sxr ON i.instrument_id = sxr.instrument_id " +
                "INNER JOIN symbologies s ON s.symbology_id = sxr.symbology_id " +
                "WHERE sxr.Symbol = @symbol " +
                    "AND s.type_code = @type_code " +
                    $"AND sxr.valid_to = '{TemporalDefaults.CurrentSentinelSql}'";
        var command = new CommandDefinition(sql, parameters: new {type_code = symbology, symbol = symbol}, cancellationToken:cancellationToken);

       return await _dbConnection.QuerySingleOrDefaultAsync<ResolvedSymbol>(command);
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

    public Task UpdateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateSymbolValidToAsync(Guid symbolXRefId, DateOnly validTo, CancellationToken cancellationToken = default)
    {
        var symbol = await _dbContext.SymbolXRefs.FindAsync(symbolXRefId, cancellationToken);
        if (symbol is not null)
        {
            symbol.ValidTo = validTo;
            symbol.LastUpdatedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

        }
  
    }

    public Task UpdateVendorInterfaceSymbolAsync(Guid vendorInterfaceSymbolXRefId, bool isActive, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
