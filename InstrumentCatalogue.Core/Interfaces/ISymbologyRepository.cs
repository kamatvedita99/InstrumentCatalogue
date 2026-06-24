using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Core.Interfaces;

public interface ISymbologyRepository
{
    Task<int> CreateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default);

    Task UpdateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default);

    Task<Symbology?> GetSymbologyByIdAsync(int symbologyId,  CancellationToken cancellationToken = default);

    Task<ICollection<Symbology>> GetSymbologiesAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken = default);

    Task UpdateSymbolAsync(SymbolXRef symbolXRef, CancellationToken cancellationToken= default);

    Task UpdateSymbolValidToAsync(Guid symbolXRefId, DateOnly validTo, CancellationToken cancellationToken = default);

    Task<SymbolXRef?> GetSymbolByIdAsync(Guid symbolXRefId, CancellationToken cancellationToken = default);

    Task<PagedResult<SymbolXRef>> GetSymbolsAsync(int symbologyId, CancellationToken cancellationToken = default);

    Task<Guid> CreateVendorInterfaceSymbolAsync(VendorInterfaceSymbolXRef xref, CancellationToken cancellationToken = default);
    
    Task UpdateVendorInterfaceSymbolAsync(Guid vendorInterfaceSymbolXRefId, bool isActive, CancellationToken cancellationToken = default);


    Task<ICollection<Symbology>> GetSymbologiesByTypeCodeAsync(ICollection<string> symbologyCodes,  CancellationToken cancellationToken = default);

}
