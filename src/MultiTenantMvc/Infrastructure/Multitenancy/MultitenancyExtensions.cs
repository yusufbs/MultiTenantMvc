using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenantMvc.Infrastructure.Multitenancy;

public static class MultitenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ITenantStore, InMemoryTenantStore>();
        services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();

        // Register scoped accessor to resolve current tenant directly from TenantContext
        services.AddScoped<TenantInfo>(sp =>
        {
            var accessor = sp.GetRequiredService<ITenantContextAccessor>();
            return accessor.TenantContext?.CurrentTenant 
                ?? sp.GetRequiredService<ITenantStore>().GetDefaultTenant();
        });

        return services;
    }

    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
