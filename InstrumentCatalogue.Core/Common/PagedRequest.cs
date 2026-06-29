namespace InstrumentCatalogue.Core.Common;

public class PagedRequest<TFilter>
{
    public TFilter Filter { get; set; } = default!;

    public int PageSize { get; set; } = 20;

    public string? Cursor { get; set; }


}
