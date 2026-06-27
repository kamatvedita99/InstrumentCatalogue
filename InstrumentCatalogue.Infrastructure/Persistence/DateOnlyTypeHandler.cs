using Dapper;
using System.Data;

namespace InstrumentCatalogue.Infrastructure.Persistence;

// Npgsql hands Dapper a DateTime for Postgres `date` columns; Dapper has no
// built-in DateTime -> DateOnly conversion, so without this it throws
// InvalidCastException on every DateOnly/DateOnly? column.
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);

    public override void SetValue(IDbDataParameter parameter, DateOnly value) =>
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
}

public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value) =>
        value is DateTime dt ? DateOnly.FromDateTime(dt) : null;

    public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
        parameter.Value = value is null ? DBNull.Value : value.Value.ToDateTime(TimeOnly.MinValue);
}