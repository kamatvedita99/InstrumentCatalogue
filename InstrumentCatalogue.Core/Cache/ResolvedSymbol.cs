using InstrumentCatalogue.Core.Enums;
namespace InstrumentCatalogue.Core.Cache;

public record ResolvedSymbol
{
    public Guid InstrumentId { get; init; }
        
    public InstrumentType Type {  get; init; }

    public string Name { get; init; } = string.Empty;
        
}
