namespace MultiTenantMvc.Infrastructure.Multitenancy;

public class TenantContext
{
    public required TenantInfo CurrentTenant { get; init; }
    public required string ResolutionSource { get; init; }
}
