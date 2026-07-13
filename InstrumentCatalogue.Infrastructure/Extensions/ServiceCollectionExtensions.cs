using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Infrastructure.Cache;
using InstrumentCatalogue.Infrastructure.Configuration;
using InstrumentCatalogue.Infrastructure.Persistence;
using InstrumentCatalogue.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;
using StackExchange.Redis;
using Dapper;
using Npgsql;
using System.Data;


namespace InstrumentCatalogue.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //repositories
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<ISymbologyRepository, SymbologyRepository>();
        services.AddScoped<IInstrumentRepository, InstrumentRepository>();

        //database - ef core
        services.AddDbContext<CatalogueDbContext>(options => options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

        //database -dapper
        services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(
        configuration.GetConnectionString("DefaultConnection")));
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());

        //memory cache
        services.AddMemoryCache();
        services.AddSingleton<ISymbologyCache, MemorySymbologyCache>();

        //redis
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection settings not configured");

        // Redis eviction policy: volatile-lru (set via Redis Cloud dashboard)
        // All cached keys have explicit 24h TTL, so volatile-lru is functionally
        // equivalent to allkeys-lru for this implementation.
        // allkeys-lru would be preferred in production (Redis Cloud paid tier).
        services.AddSingleton<IConnectionMultiplexer>(sp =>
                            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<ISymbolResolutionCache, SymbolResolutionCache>();

        //Polly
        var redisSettings = configuration
            .GetSection("Resilience:Redis")
            .Get<RedisResilienceConfiguration>()
            ?? throw new InvalidOperationException("Redis resilience settings not configured");

        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>().Handle<RedisTimeoutException>(),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            MaxRetryAttempts = redisSettings.RetryCount,
            Delay = TimeSpan.FromMilliseconds(redisSettings.RetryDelayMs),
        };

        var circuitBreakerOptions = new CircuitBreakerStrategyOptions
        {
            FailureRatio = redisSettings.FailureRatio,
            SamplingDuration = TimeSpan.FromSeconds(redisSettings.SamplingDurationSeconds),
            MinimumThroughput = redisSettings.MinimumThroughput,
            BreakDuration = TimeSpan.FromSeconds(redisSettings.BreakDurationSeconds),
            ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>().Handle<RedisTimeoutException>()
        };
        var resiliencePipeline = new ResiliencePipelineBuilder().AddRetry(retryOptions).AddCircuitBreaker(circuitBreakerOptions).Build();
        services.AddSingleton(resiliencePipeline);

        //health checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!, tags: new[] { "ready" })
            .AddRedis(configuration.GetConnectionString("Redis")!, tags: new[] {"ready"});


    }
}
