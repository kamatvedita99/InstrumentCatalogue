using FluentValidation;
using InstrumentCatalogue.Application.Services;
using InstrumentCatalogue.Application.Validators.Vendor;
using Microsoft.Extensions.DependencyInjection;

namespace InstrumentCatalogue.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<ISymbologyService, SymbologyService>();
        services.AddScoped<IInstrumentService, InstrumentService>();
        services.AddValidatorsFromAssemblyContaining<CreateVendorRequestValidator>();


    }
}
