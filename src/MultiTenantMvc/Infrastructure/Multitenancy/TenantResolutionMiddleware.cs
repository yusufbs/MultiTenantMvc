using Microsoft.AspNetCore.Http;

namespace MultiTenantMvc.Infrastructure.Multitenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private const string TenantCookieName = "X-Selected-Tenant";

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantStore tenantStore, ITenantContextAccessor tenantAccessor)
    {
        TenantInfo? resolvedTenant = null;
        string resolutionSource = "Default Fallback";

        // 1. Explicit Query String (?tenant=alpha or ?tenant=beta)
        if (context.Request.Query.TryGetValue("tenant", out var tenantQuery) && !string.IsNullOrWhiteSpace(tenantQuery))
        {
            var match = tenantStore.GetTenant(tenantQuery.ToString().Trim());
            if (match != null)
            {
                resolvedTenant = match;
                resolutionSource = $"Query Parameter (?tenant={match.Id})";
                
                // Persist selection into cookie for session continuity
                context.Response.Cookies.Append(TenantCookieName, match.Id, new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
            }
        }

        // 2. HTTP Request Header (X-Tenant: alpha)
        if (resolvedTenant == null && context.Request.Headers.TryGetValue("X-Tenant", out var tenantHeader) && !string.IsNullOrWhiteSpace(tenantHeader))
        {
            var match = tenantStore.GetTenant(tenantHeader.ToString().Trim());
            if (match != null)
            {
                resolvedTenant = match;
                resolutionSource = $"HTTP Header (X-Tenant: {match.Id})";
            }
        }

        // 3. Subdomain Resolution (e.g., alpha.localhost:5000 -> "alpha")
        if (resolvedTenant == null)
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length > 1 && !string.Equals(parts[0], "www", StringComparison.OrdinalIgnoreCase))
            {
                var candidateSubdomain = parts[0];
                var match = tenantStore.GetTenant(candidateSubdomain);
                if (match != null)
                {
                    resolvedTenant = match;
                    resolutionSource = $"Host Subdomain ({candidateSubdomain})";
                }
            }
        }

        // 4. Cookie Persistence (from prior query switch)
        if (resolvedTenant == null && context.Request.Cookies.TryGetValue(TenantCookieName, out var cookieTenant) && !string.IsNullOrWhiteSpace(cookieTenant))
        {
            var match = tenantStore.GetTenant(cookieTenant);
            if (match != null)
            {
                resolvedTenant = match;
                resolutionSource = $"Session Cookie ({match.Id})";
            }
        }

        // 5. Default Fallback
        if (resolvedTenant == null)
        {
            resolvedTenant = tenantStore.GetDefaultTenant();
            resolutionSource = "Global Base Fallback";
        }

        var tenantContext = new TenantContext
        {
            CurrentTenant = resolvedTenant,
            ResolutionSource = resolutionSource
        };

        // Store in HttpContext.Items and the ambient accessor
        context.Items["TenantContext"] = tenantContext;
        tenantAccessor.TenantContext = tenantContext;

        await _next(context);
    }
}
