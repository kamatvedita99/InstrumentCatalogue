using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Application.DTOs.Instrument
{
    public class CreateBondRefRequest
    {
        public decimal FaceValue { get; set; }

        public decimal? CouponRate { get; set; }

        public CouponFrequency? CouponFrequency { get; set; }

        public string? Issuer { get; set; }

        public DateOnly? IssueDate { get; set; }

        public DateOnly? MaturityDate { get; set; }

        public string? CreditRating { get; set; }

        public BondType? BondType { get; set; }

        public BondStructure? BondStructure { get; set; }

        public decimal? Duration { get; set; }
    }
}
