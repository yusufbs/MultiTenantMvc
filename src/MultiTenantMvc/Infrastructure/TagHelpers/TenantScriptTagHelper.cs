using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MultiTenantMvc.Infrastructure.Multitenancy;

namespace MultiTenantMvc.Infrastructure.TagHelpers;

[HtmlTargetElement("tenant-script", Attributes = "src")]
public class TenantScriptTagHelper : TagHelper
{
    private readonly IWebHostEnvironment _env;
    private readonly ITenantContextAccessor _tenantAccessor;

    [HtmlAttributeName("src")]
    public string Src { get; set; } = string.Empty;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public TenantScriptTagHelper(IWebHostEnvironment env, ITenantContextAccessor tenantAccessor)
    {
        _env = env;
        _tenantAccessor = tenantAccessor;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "script";

        var tenantId = _tenantAccessor.TenantContext?.CurrentTenant.Id ?? "base";
        var fileName = Src.TrimStart('/');

        // 1. Check if tenant-specific override script exists in wwwroot/tenants/{tenantId}/js/{fileName}
        var tenantScriptRelativePath = $"tenants/{tenantId}/js/{fileName}";
        var tenantFileInfo = _env.WebRootFileProvider.GetFileInfo(tenantScriptRelativePath);

        string finalScriptUrl;
        bool isOverride;

        if (tenantFileInfo.Exists)
        {
            finalScriptUrl = $"/{tenantScriptRelativePath}";
            isOverride = true;
        }
        else
        {
            // 2. Fall back to base wwwroot/js/{fileName}
            finalScriptUrl = $"/js/{fileName}";
            isOverride = false;
        }

        output.Attributes.SetAttribute("src", finalScriptUrl);
        output.Attributes.SetAttribute("data-tenant", tenantId);
        output.Attributes.SetAttribute("data-is-tenant-override", isOverride ? "true" : "false");
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}
