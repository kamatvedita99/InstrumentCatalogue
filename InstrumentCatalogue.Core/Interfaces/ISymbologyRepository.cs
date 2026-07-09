using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Core.Interfaces;

public interface ISymbologyRepository
{
    Task<int> CreateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default);

    Task UpdateSymbologyAsync(Symbology symbology, CancellationToken cancellationToken = default);

    Task<Symbology?> GetSymbologyByIdAsync(int symbologyId,  CancellationToken cancellationToken = default);

    Task<ICollection<Symbology>> GetSymbologiesAsync(CancellationToken cancellationToken = default);

    Task<ICollection<Symbology>> GetSymbologiesByTypeCodeAsync(ICollection<string> symbologyCodes,  CancellationToken cancellationToken = default);

}
