namespace IceSync.Workers.Configurations;

public record SynchronizationWorkerOptions
{
    public required int IntervalSeconds { get; init; }
}
