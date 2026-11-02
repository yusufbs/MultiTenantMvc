namespace MultiTenantMvc.Infrastructure.Multitenancy;

public class InMemoryTenantStore : ITenantStore
{
    private readonly Dictionary<string, TenantInfo> _tenants = new(StringComparer.OrdinalIgnoreCase)
    {
        ["base"] = new TenantInfo
        {
            Id = "base",
            Name = "Base Platform (Global Core)",
            Description = "Standard multi-tenant default base instance with global logic and fallback templates.",
            Hostname = "localhost",
            PrimaryColor = "#2563eb", // Royal Blue
            SecondaryColor = "#1e40af",
            BadgeText = "Default Base Site",
            IsBase = true
        },
        ["alpha"] = new TenantInfo
        {
            Id = "alpha",
            Name = "Tenant Alpha (Enterprise Solutions)",
            Description = "High-volume B2B tenant with custom backend C# pricing logic and custom Razor (.cshtml) layout.",
            Hostname = "alpha.localhost",
            PrimaryColor = "#7c3aed", // Violet / Purple
            SecondaryColor = "#5b21b6",
            BadgeText = "Tenant Alpha: Custom .cs & .cshtml",
            IsBase = false
        },
        ["beta"] = new TenantInfo
        {
            Id = "beta",
            Name = "Tenant Beta (Retail Network)",
            Description = "Consumer-facing retail tenant using standard base C# logic & views, with a custom JavaScript (.js) frontend override.",
            Hostname = "beta.localhost",
            PrimaryColor = "#059669", // Emerald Green
            SecondaryColor = "#047857",
            BadgeText = "Tenant Beta: Custom .js Override",
            IsBase = false
        }
    };

    public TenantInfo? GetTenant(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        // Try direct Id lookup (e.g. "alpha", "beta", "base")
        if (_tenants.TryGetValue(identifier, out var tenantById))
            return tenantById;

        // Try hostname lookup (e.g. "alpha.localhost")
        return _tenants.Values.FirstOrDefault(t => 
            string.Equals(t.Hostname, identifier, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<TenantInfo> GetAllTenants() => _tenants.Values.ToList().AsReadOnly();

    public TenantInfo GetDefaultTenant() => _tenants["base"];
}
