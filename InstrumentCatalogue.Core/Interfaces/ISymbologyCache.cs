using InstrumentCatalogue.Core.Cache;

namespace InstrumentCatalogue.Core.Interfaces;

public interface ISymbologyCache
{
    public bool TryGet(string typeCode, out int symbologyId);

    public void Set(string typeCode, int symbologyId);
}
