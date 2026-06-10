namespace InstrumentCatalogue.Core.Common;

public class PagedResult<T>
{
    public ICollection<T> Items { get; set; } = new List<T>();

    public string? NextCursor { get; set; }

    public bool HasMore => NextCursor != null;
}
