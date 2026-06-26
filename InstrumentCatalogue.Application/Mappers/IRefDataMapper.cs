using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Application.Mappers;

public interface IRefDataMapper
{
    public IInstrumentRefData Map(CreateInstrumentRequest request);

}
