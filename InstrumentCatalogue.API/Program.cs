using InstrumentCatalogue.Application.Services;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Infrastructure.Persistence;
using InstrumentCatalogue.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using FluentValidation;
using InstrumentCatalogue.API.Middleware;
using InstrumentCatalogue.Application.Validators.Vendor;
using Dapper;
using InstrumentCatalogue.Infrastructure.Cache;
using StackExchange.Redis;
using Polly.Retry;
using Polly;
using Polly.CircuitBreaker;
using InstrumentCatalogue.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(allowIntegerValues: false)));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVendorRequestValidator>();

builder.Services.AddDbContext<CatalogueDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISymbologyCache, MemorySymbologyCache>();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection settings not configured");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<ISymbolResolutionCache, SymbolResolutionCache>();

var redisSettings = builder.Configuration
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
builder.Services.AddSingleton(resiliencePipeline);

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());

builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorService, VendorService>();

builder.Services.AddScoped<ISymbologyRepository, SymbologyRepository>();
builder.Services.AddScoped<ISymbologyService, SymbologyService>();

builder.Services.AddScoped<IInstrumentService, InstrumentService>();
builder.Services.AddScoped<IInstrumentRepository, InstrumentRepository>();


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
