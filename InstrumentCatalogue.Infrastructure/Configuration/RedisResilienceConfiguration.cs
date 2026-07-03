namespace InstrumentCatalogue.Infrastructure.Configuration;

public class RedisResilienceConfiguration
{
    public int RetryCount { get; set; }
    public int RetryDelayMs { get; set; }
    public double FailureRatio { get; set; }
    public int MinimumThroughput { get; set; }
    public int SamplingDurationSeconds { get; set; }
    public int BreakDurationSeconds { get; set; }
}