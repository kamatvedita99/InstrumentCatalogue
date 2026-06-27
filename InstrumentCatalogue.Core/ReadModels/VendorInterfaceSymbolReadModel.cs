namespace InstrumentCatalogue.Core.ReadModels;

public class VendorInterfaceSymbolReadModel
{
        public Guid VendorInterfaceSymbolXRefId { get; set; }
        public Guid SymbolXRefId { get; set; }
        public int VendorInterfaceId { get; set; }
        public string VendorInterfaceName { get; set; } = string.Empty;
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
    
}
