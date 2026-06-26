using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Application.Extensions;

namespace InstrumentCatalogue.Application.Mappers;

public static class BondRefDataMapper
{
    public static BondRefData ToDomain(CreateInstrumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var bondRefRequest = request.BondRef;

        ArgumentNullException.ThrowIfNull(bondRefRequest);

        var bondRef = new BondRefData
        {
            FaceValue = bondRefRequest.FaceValue,
            BondStructure = bondRefRequest.BondStructure,
            BondType = bondRefRequest.BondType,
            CreditRating = bondRefRequest.CreditRating,
            CouponFrequency = bondRefRequest.CouponFrequency,
            CouponRate = bondRefRequest.CouponRate,
            MaturityDate = bondRefRequest.MaturityDate,
            Duration = bondRefRequest.Duration,
            IssueDate = bondRefRequest.IssueDate,
            Issuer = bondRefRequest.Issuer,
        };

        bondRef.StampCreated();
        return bondRef;

    }

    public static BondRefResponse ToResponse(BondRefData bondRefData)
    {
        ArgumentNullException.ThrowIfNull(bondRefData);

        return new BondRefResponse
        {
            InstrumentId = bondRefData.InstrumentId,
            FaceValue = bondRefData.FaceValue,
            Issuer = bondRefData?.Issuer,
            BondStructure = bondRefData?.BondStructure,
            BondType = bondRefData?.BondType,
            CouponFrequency = bondRefData?.CouponFrequency,
            CouponRate = bondRefData?.CouponRate,
            CreditRating = bondRefData?.CreditRating,
            MaturityDate = bondRefData?.MaturityDate,
            Duration = bondRefData?.Duration,
            IssueDate = bondRefData?.IssueDate,


        };
    }
}
