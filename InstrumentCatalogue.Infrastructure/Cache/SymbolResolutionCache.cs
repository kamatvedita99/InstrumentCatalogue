using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace InstrumentCatalogue.Infrastructure.Cache;

public class SymbolResolutionCache : ISymbolResolutionCache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private readonly ILogger<SymbolResolutionCache> _logger;   

    private string GetCacheKey(int symbologyId, string symbol)
    {
        return $"symbologyId:{symbologyId}:symbol:{symbol.ToUpperInvariant()}";
    }
    public SymbolResolutionCache(IConnectionMultiplexer connectionMultiplexer, ILogger<SymbolResolutionCache> logger)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }
    public async Task DeleteAsync(int symbologyId, string symbol, CancellationToken cancellationToken)
    {
        var redisDbConnection = _connectionMultiplexer.GetDatabase();
        await redisDbConnection.KeyDeleteAsync(GetCacheKey(symbologyId, symbol));
    }

    public async Task<ResolvedSymbol?> GetAsync(int symbologyId, string symbol, CancellationToken cancellationToken)
    {
        var redisDbConnection = _connectionMultiplexer.GetDatabase();
        var value = await redisDbConnection.StringGetAsync(GetCacheKey(symbologyId, symbol));
        
        if(value == RedisValue.Null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<ResolvedSymbol?>(value);
        }

        catch(JsonException ex)
        {
            _logger.LogWarning(ex, "Error occurred during cache deserialization for symbol resoultion");
            return default;
        }
    }

    public async Task SetAsync(int symbologyId, string symbol, ResolvedSymbol resolvedSymbol, CancellationToken cancellationToken)
    {
        var redisDbConnection = _connectionMultiplexer.GetDatabase();
        await redisDbConnection.StringSetAsync(GetCacheKey(symbologyId, symbol), JsonSerializer.Serialize(resolvedSymbol), TimeSpan.FromHours(24));
       
    }
}
