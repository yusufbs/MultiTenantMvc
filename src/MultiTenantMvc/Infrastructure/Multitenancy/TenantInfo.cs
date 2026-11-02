namespace MultiTenantMvc.Infrastructure.Multitenancy;

public class TenantInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Hostname { get; init; }
    public required string PrimaryColor { get; init; }
    public required string SecondaryColor { get; init; }
    public required string BadgeText { get; init; }
    public bool IsBase { get; init; }
}
