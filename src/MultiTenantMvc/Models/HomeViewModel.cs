using MultiTenantMvc.Infrastructure.Multitenancy;
using MultiTenantMvc.Services;

namespace MultiTenantMvc.Models;

public class HomeViewModel
{
    public required TenantInfo CurrentTenant { get; init; }
    public required string ResolutionSource { get; init; }
    public required DiscountResult Discount { get; init; }
    public required string RenderedViewSource { get; init; }
    public required string ScriptFileExpected { get; init; }
    public required bool ScriptIsOverridden { get; init; }
    public required IReadOnlyCollection<TenantInfo> AvailableTenants { get; init; }
}
