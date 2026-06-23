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
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVendorRequestValidator>();

builder.Services.AddDbContext<CatalogueDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorService, VendorService>();

builder.Services.AddScoped<ISymbologyRepository, SymbologyRepository>();
builder.Services.AddScoped<ISymbologyService, SymbologyService>();

builder.Services.AddSingleton( new Dictionary<InstrumentType, IRefDataMapper>
{
    { InstrumentType.Bond, new BondRefDataMapper() },
    { InstrumentType.Equity, new EquityRefDataMapper() },
    { InstrumentType.ETF, new EtfRefDataMapper() }
});

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
