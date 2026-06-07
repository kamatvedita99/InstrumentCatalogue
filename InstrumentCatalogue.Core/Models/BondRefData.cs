using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Core.Models;

public class BondRefData
{
    public Guid InstrumentId { get; set; }

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

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }
   
}
