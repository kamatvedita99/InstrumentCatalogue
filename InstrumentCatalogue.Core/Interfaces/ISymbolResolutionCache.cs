using InstrumentCatalogue.Core.Cache;

namespace InstrumentCatalogue.Core.Interfaces;

public interface ISymbolResolutionCache
{
    public Task SetAsync(int symbologyId, string symbol, ResolvedSymbol resolvedSymbol, CancellationToken cancellationToken);

    public Task<ResolvedSymbol?> GetAsync(int  symbologyId, string symbol, CancellationToken cancellationToken);

    public Task DeleteAsync(int symbologyId, string symbol, CancellationToken cancellationToken);
}
