using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Core.ReadModels;

public class InstrumentSearchResult
{
    public Guid InstrumentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public InstrumentType Type { get; set; }

    public InstrumentStatus Status { get; set; }

    public string Exchange { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string DisplaySymbol { get; set; } = string.Empty;

    public string DisplaySymbology { get; set; } = string.Empty;

    public bool MatchedOnSymbol { get; set; }

    public EquitySearchContext? Equity { get; set; }

    public BondSearchContext? Bond { get; set; }

    public EtfSearchContext? Etf { get; set; }
}

public class EquitySearchContext
{
    public string? Sector { get; set; }
    public string? Industry { get; set; }
}

public class BondSearchContext
{
    public DateOnly? MaturityDate { get; set; }
    public string? Issuer { get; set; }
    public string? CreditRating { get; set; }
}

public class EtfSearchContext
{
    public string? UnderlyingIndex { get; set; }
    public decimal? ExpenseRatio { get; set; }
}

