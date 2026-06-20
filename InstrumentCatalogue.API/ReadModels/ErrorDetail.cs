namespace InstrumentCatalogue.API.ReadModels;

public class ErrorDetail
{
    public string Message { get; set; } = string.Empty;

    public Dictionary<string, List<string>> Errors { get; set; } = new();

    public string TraceId { get; set; } = string.Empty;    

}
