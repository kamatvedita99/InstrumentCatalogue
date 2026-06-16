using InstrumentCatalogue.Application.Services;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Infrastructure.Persistence;
using InstrumentCatalogue.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using FluentValidation;
using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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


var app = builder.Build();

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
