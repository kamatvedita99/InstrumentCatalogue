using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Core.Filters;

public class BondFilter
{
    public DateOnly? MaturityBefore { get; set; }
    public DateOnly? MaturityAfter { get; set; }
    public DateOnly? IssueDate { get; set; }
    public string? CreditRating { get; set; }
    public BondType? BondType { get; set; }
    public BondStructure? BondStructure { get; set; }
    public string? Issuer { get; set; }
}