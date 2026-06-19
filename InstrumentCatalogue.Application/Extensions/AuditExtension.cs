using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Application.Extensions;

public static class AuditExtension
{
    public static T StampCreated<T>(this T entity) where T: ITimeStampAudit
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedAtUtc = DateTime.UtcNow;
        entity.LastUpdatedAtUtc = DateTime.UtcNow;

        return entity;

    }

    public static T StampUpdated<T>(this T entity) where T: ITimeStampAudit
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.LastUpdatedAtUtc = DateTime.UtcNow;
        return entity;
    }
}
