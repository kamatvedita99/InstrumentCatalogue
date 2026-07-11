using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Exceptions;

namespace InstrumentCatalogue.Core.Rules;

public static class InstrumentStatusTransitionRule
{
    private static Dictionary<InstrumentStatus, List<InstrumentStatus>> _validStatusTransitionDictionary = new()
    {
        { InstrumentStatus.Active, new List<InstrumentStatus>(){InstrumentStatus.Delisted, InstrumentStatus.Suspended} },
        { InstrumentStatus.Pending, new List<InstrumentStatus>(){InstrumentStatus.Active, InstrumentStatus.Delisted} },
        { InstrumentStatus.Suspended, new List<InstrumentStatus>(){InstrumentStatus.Active, InstrumentStatus.Delisted} },
    };
    public static void ValidateTransition(InstrumentStatus fromStatus, InstrumentStatus toStatus)
    {
        _validStatusTransitionDictionary.TryGetValue(fromStatus, out var validTransitionStatuses);

        if(validTransitionStatuses is null)
            throw new InstrumentStatusTransitionException($"{fromStatus.ToString()} is a terminal status. No valid transitions available.");

        if(validTransitionStatuses.Count == 0)
            throw new InstrumentStatusTransitionException($"Cannot transition the instrument from {fromStatus.ToString()} to {toStatus.ToString()}. No valid transitions available.");

        if(!validTransitionStatuses.Contains(toStatus))
            throw new InstrumentStatusTransitionException($"Cannot transition the instrument from {fromStatus.ToString()} to {toStatus.ToString()}. Valid transitions are [{string.Join(",", validTransitionStatuses)}].");
    }
}
