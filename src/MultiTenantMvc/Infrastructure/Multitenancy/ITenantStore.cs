namespace MultiTenantMvc.Infrastructure.Multitenancy;

public interface ITenantStore
{
    TenantInfo? GetTenant(string identifier);
    IReadOnlyCollection<TenantInfo> GetAllTenants();
    TenantInfo GetDefaultTenant();
}
