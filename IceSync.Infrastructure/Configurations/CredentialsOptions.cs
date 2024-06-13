namespace IceSync.Infrastructure.Configurations;

public record CredentialsOptions
{
    public required string CompanyID { get; init; }
    public required string UserID { get; init; }
    public required string UserSecret { get; init; }
}
