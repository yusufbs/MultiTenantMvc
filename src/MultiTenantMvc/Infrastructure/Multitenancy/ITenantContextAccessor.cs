using Microsoft.AspNetCore.Http;

namespace MultiTenantMvc.Infrastructure.Multitenancy;

public interface ITenantContextAccessor
{
    TenantContext? TenantContext { get; set; }
}

public class TenantContextAccessor : ITenantContextAccessor
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private TenantContext? _ambientContext;

    public TenantContextAccessor(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantContext? TenantContext
    {
        get
        {
            if (_httpContextAccessor?.HttpContext?.Items.TryGetValue("TenantContext", out var obj) == true && obj is TenantContext tc)
            {
                return tc;
            }
            return _ambientContext;
        }
        set
        {
            if (_httpContextAccessor?.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items["TenantContext"] = value;
            }
            _ambientContext = value;
        }
    }
}
