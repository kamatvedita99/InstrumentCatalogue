using InstrumentCatalogue.Core.Enums;
namespace InstrumentCatalogue.Core.Cache;

public record ResolvedSymbol(Guid InstrumentId, InstrumentType InstrumentType, string Name);
