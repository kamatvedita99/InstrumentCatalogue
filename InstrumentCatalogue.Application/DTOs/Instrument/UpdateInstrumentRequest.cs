namespace InstrumentCatalogue.Application.DTOs.Instrument
{
    public class UpdateInstrumentRequest
    {
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Currency { get; set; }
        public string? Exchange { get; set; }
        public DateOnly? ListedDate { get; set; }

        public UpdateBondRefRequest? BondRef { get; set; }

        public UpdateEquityRefRequest? EquityRef { get; set; }

        public UpdateEtfRefRequest? EtfRef { get; set; }

    }
}
