using InstrumentCatalogue.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace InstrumentCatalogue.Infrastructure.Cache;

public class MemorySymbologyCache : ISymbologyCache
{
    private readonly IMemoryCache _memoryCache;

    public MemorySymbologyCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    private string GetCacheKey(string typeCode)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeCode);

        return $"symbology:{typeCode.ToUpperInvariant()}";
    }
    public void Set(string typeCode, int symbologyId)
    {
        _memoryCache.Set(GetCacheKey(typeCode), symbologyId, DateTime.UtcNow.AddDays(1));
    }

    public bool TryGet(string typeCode, out int symbologyId)
    {
        return _memoryCache.TryGetValue<int>(GetCacheKey(typeCode), out symbologyId);
    }

}
