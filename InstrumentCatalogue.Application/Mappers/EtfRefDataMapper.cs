using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Interfaces.Shared;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public class EtfRefDataMapper : IRefDataMapper
{
    public IInstrumentRefData Map(CreateInstrumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var etfRefRequest = request.EtfRef;
        ArgumentNullException.ThrowIfNull(etfRefRequest);

        var etfRef = new EtfRefData
        {
            FundManager = etfRefRequest.FundManager,
            UnderlyingIndex = etfRefRequest.UnderlyingIndex,
            ReplicationType = etfRefRequest.ReplicationType,
            DistributionFrequency = etfRefRequest.DistributionFrequency,
            InceptionDate = etfRefRequest.InceptionDate,
            ExpenseRatio = etfRefRequest.ExpenseRatio,
        };

        etfRef.StampCreated();
        return etfRef;
    }

    public static EtfRefResponse ToResponse(EtfRefData etfRefData)
    {
        ArgumentNullException.ThrowIfNull(etfRefData);

        return new EtfRefResponse
        {
            InstrumentId = etfRefData.InstrumentId,
            FundManager = etfRefData.FundManager,
            ReplicationType = etfRefData.ReplicationType,
            DistributionFrequency = etfRefData.DistributionFrequency,
            InceptionDate = etfRefData.InceptionDate,
            UnderlyingIndex = etfRefData.UnderlyingIndex,
            ExpenseRatio = etfRefData.ExpenseRatio
        };
    }
}