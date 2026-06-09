using InstrumentCatalogue.Core.Enums;


namespace InstrumentCatalogue.Core.Models;

    public class Instrument
    {
        public Guid InstrumentId { get; set; }

        public string Name { get; set; } = string.Empty;

        public InstrumentType Type { get; set; }

        public string Exchange { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public string Country {  get; set; } = string.Empty;

        public DateOnly? ListedDate { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime LastUpdatedAtUtc { get; set; }

        //Navigation properties
        public EquityRefData? EquityRefData { get; set; }

        public BondRefData? BondRefData { get; set; }

        public EtfRefData? EtfRefData { get; set; }

        public ICollection<SymbolXRef> Symbols { get; set; } = new List<SymbolXRef>();

        public ICollection<InstrumentStatusHistory> StatusHistory { get; set; } = new List<InstrumentStatusHistory>();


    }

