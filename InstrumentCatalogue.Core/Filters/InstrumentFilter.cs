using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Core.Filters;

public class InstrumentFilter
{
    public InstrumentType? Type { get; set; }
    public InstrumentStatus? Status { get; set; } = InstrumentStatus.Active;
    public string? Exchange { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? Name { get; set; }
    public DateOnly? ListedDateFrom { get; set; }
    public DateOnly? ListedDateTo { get; set; }
    public BondFilter? BondFilter { get; set; }
    public EquityFilter? EquityFilter { get; set; }
    public EtfFilter? EtfFilter { get; set; }
}
