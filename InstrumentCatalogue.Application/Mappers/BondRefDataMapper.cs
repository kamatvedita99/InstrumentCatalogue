using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Application.Mappers;

public class BondRefDataMapper : IRefDataMapper
{
    public IInstrumentRefData Map(CreateInstrumentRequest request)
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
}
