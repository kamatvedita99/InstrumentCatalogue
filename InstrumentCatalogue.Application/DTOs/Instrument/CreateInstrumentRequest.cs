namespace InstrumentCatalogue.Application.DTOs.Instrument;

public class CreateInstrumentRequest
{
    public string Name { get; set; } = string.Empty;

    public InstrumentType Type { get; set; }

    public string Exchange { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public DateOnly? ListedDate { get; set; }

    public ICollection<CreateInstrumentSymbolRequest> Symbols { get; set; } = new();

    public CreateBondRefRequest? BondRef { get; set; }

    public CreateEquityRefRequest? EquityRef { get; set; }

    public CreateEtfRefRequest? EtfRef { get; set; }

    public string VendorName { get; set; } = string.Empty;

    public string InterfaceName { get; set; } = string.Empty;

}
