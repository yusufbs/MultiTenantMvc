using Microsoft.AspNetCore.Mvc.Razor;
using MultiTenantMvc.Infrastructure.Multitenancy;

namespace MultiTenantMvc.Infrastructure.ViewEngine;

public class TenantViewLocationExpander : IViewLocationExpander
{
    private const string ValueKey = "tenant";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        context.Values ??= new Dictionary<string, string>();

        // Retrieve resolved tenant from HttpContext.Items or TenantContext
        if (context.ActionContext?.HttpContext?.Items.TryGetValue("TenantContext", out var tcObj) == true 
            && tcObj is TenantContext tenantContext)
        {
            context.Values[ValueKey] = tenantContext.CurrentTenant.Id;
        }
        else
        {
            context.Values[ValueKey] = "base";
        }
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.Values != null && context.Values.TryGetValue(ValueKey, out var tenantId) && !string.IsNullOrWhiteSpace(tenantId))
        {
            // First check tenant-specific view folders:
            // e.g. /Tenants/alpha/Views/{1}/{0}.cshtml
            // and  /Tenants/alpha/Views/Shared/{0}.cshtml
            yield return $"/Tenants/{tenantId}/Views/{{1}}/{{0}}.cshtml";
            yield return $"/Tenants/{tenantId}/Views/Shared/{{0}}.cshtml";
        }

        // Fall back to standard MVC locations:
        // /Views/{1}/{0}.cshtml
        // /Views/Shared/{0}.cshtml
        foreach (var location in viewLocations)
        {
            yield return location;
        }
    }
}
