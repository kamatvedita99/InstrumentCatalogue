using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;
using InstrumentCatalogue.Application.DTOs.SymbolXRef;
using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Application.DTOs.Instrument;

public record InstrumentResponse
{
    public Guid InstrumentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public InstrumentType Type { get; set; }

    public string Exchange { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public DateOnly? ListedDate { get; set; }

    public ICollection<SymbolXRefResponse> Symbols { get; set; } = new List<SymbolXRefResponse>();

    public BondRefResponse? BondRef { get; set; }

    public EquityRefResponse? EquityRef { get; set; }

    public EtfRefResponse? EtfRef { get; set; }

    public ICollection<InstrumentStatusHistoryResponse> InstrumentStatusHistory { get; set; } = new List<InstrumentStatusHistoryResponse>();


}
