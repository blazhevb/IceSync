namespace IceSync.Infrastructure.Configurations;

public record ApiOptions
{
    public required string BaseUrl { get; init; }
}
