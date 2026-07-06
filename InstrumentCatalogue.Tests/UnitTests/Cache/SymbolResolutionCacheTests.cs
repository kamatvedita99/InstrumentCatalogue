using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Infrastructure.Cache;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;

namespace InstrumentCatalogue.Tests.UnitTests.Cache;

public class SymbolResolutionCacheTests
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private readonly ILogger<SymbolResolutionCache> _logger;

    private readonly ResiliencePipeline _resiliencePipeline;

    private readonly IDatabase _database;

    private readonly SymbolResolutionCache _sut;

    public SymbolResolutionCacheTests()
    {
       _connectionMultiplexer =  Substitute.For<IConnectionMultiplexer>();
       _database = Substitute.For<IDatabase>();
       _connectionMultiplexer.GetDatabase().Returns(_database);

        _logger = Substitute.For<ILogger<SymbolResolutionCache>>();
        _resiliencePipeline = new ResiliencePipelineBuilder()
        .Build();

        _sut = new SymbolResolutionCache(_connectionMultiplexer, _logger, _resiliencePipeline);
        
    }

    [Fact]
    public async Task TestRedisConnectionErrorSuccess()
    {
        
        var symbologyId = 2;
        var symbol = "INE467B01029";
        

        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Throws(new RedisTimeoutException("Redis timed out", new CommandStatus() { }));

        var resolvedSymbol = await _sut.GetAsync(symbologyId, symbol, default);
        
        Assert.Null(resolvedSymbol);
        
    }

    [Fact]
    public async Task TestRedisTimeoutErrorSuccess()
    {

        var symbologyId = 2;
        var symbol = "INE467B01029";


        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis down"));

        var resolvedSymbol = await _sut.GetAsync(symbologyId, symbol, default);

        Assert.Null(resolvedSymbol);

    }

    [Fact]
    public async Task TestBrokenCircuitErrorSuccess()
    {

        var symbologyId = 2;
        var symbol = "INE467B01029";


        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Throws(new BrokenCircuitException("Circuit open"));

        var resolvedSymbol = await _sut.GetAsync(symbologyId, symbol, default);

        Assert.Null(resolvedSymbol);

    }

    [Fact]
    public async Task TestKeyNotFoundSuccess()
    {

        var symbologyId = 2;
        var symbol = "INE467B01029";
        var key = $"symbologyId:{symbologyId}:symbol:{symbol}";


        _database.StringGetAsync(key, Arg.Any<CommandFlags>())
            .Returns(RedisValue.Null);

        var resolvedSymbol = await _sut.GetAsync(symbologyId, symbol, default);

        Assert.Null(resolvedSymbol);

    }
}
