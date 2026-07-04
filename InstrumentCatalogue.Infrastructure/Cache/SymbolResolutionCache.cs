using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace InstrumentCatalogue.Infrastructure.Cache;

public class SymbolResolutionCache : ISymbolResolutionCache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private readonly ILogger<SymbolResolutionCache> _logger;   

    private readonly ResiliencePipeline _resiliencePipeline;

    private string GetCacheKey(int symbologyId, string symbol)
    {
        return $"symbologyId:{symbologyId}:symbol:{symbol.ToUpperInvariant()}";
    }
    public SymbolResolutionCache(IConnectionMultiplexer connectionMultiplexer, ILogger<SymbolResolutionCache> logger, ResiliencePipeline resiliencePipeline)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _resiliencePipeline = resiliencePipeline ?? throw new ArgumentNullException(nameof(resiliencePipeline));

    }
    public async Task DeleteAsync(int symbologyId, string symbol, CancellationToken cancellationToken)
    {
        try
        {
            
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var redisDbConnection = _connectionMultiplexer.GetDatabase();
                await redisDbConnection.KeyDeleteAsync(GetCacheKey(symbologyId, symbol));
                },
                cancellationToken);
        }

        catch(BrokenCircuitException ex)
        {
            // Circuit is open — Redis is degraded. Cache deletion failure is acceptable,
            // the TTL will eventually expire the stale entry.
            _logger.LogWarning(ex, "Redis circuit is open, falling back");
         
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection failed, falling back to DB");
            
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout, falling back to DB");
            
        }

    }

    public async Task<ResolvedSymbol?> GetAsync(int symbologyId, string symbol, CancellationToken cancellationToken)
    {
        
        try
        {
            var sw = Stopwatch.StartNew();
            var value =
                await _resiliencePipeline.ExecuteAsync<RedisValue>(async ct =>

                {
                    var redisDbConnection = _connectionMultiplexer.GetDatabase();
                    return await redisDbConnection.StringGetAsync(GetCacheKey(symbologyId, symbol));
                }, cancellationToken);
        
            if(value == RedisValue.Null)
                return null;
            
            var result = JsonSerializer.Deserialize<ResolvedSymbol?>(value);
            sw.Stop();
            _logger.LogInformation("Redis GetAsync took {ElapsedMs}ms", sw.ElapsedMilliseconds);
            return result;
        }

        catch(JsonException ex)
        {
            _logger.LogWarning(ex, "Error occurred during cache deserialization for symbol resoultion");
            return default;
        }

        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Redis circuit is open, falling back");
            return default;

        }

        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection failed, falling back to DB");
            return default;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout, falling back to DB");
            return default;
        }
    }

    public async Task SetAsync(int symbologyId, string symbol, ResolvedSymbol resolvedSymbol, CancellationToken cancellationToken)
    {
        
        try
        {
            
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var redisDbConnection = _connectionMultiplexer.GetDatabase();
                await redisDbConnection.StringSetAsync(GetCacheKey(symbologyId, symbol), JsonSerializer.Serialize(resolvedSymbol), TimeSpan.FromHours(24));
            }, cancellationToken);

        }
        
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Redis circuit is open, falling back"); 

        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection failed, falling back to DB");
            
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout, falling back to DB");
           
        }

    }
}
