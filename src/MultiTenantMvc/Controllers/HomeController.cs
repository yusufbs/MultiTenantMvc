using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiTenantMvc.Infrastructure.Multitenancy;
using MultiTenantMvc.Models;
using MultiTenantMvc.Services;

namespace MultiTenantMvc.Controllers;

public class HomeController : Controller
{
    private readonly ITenantDiscountService _discountService;
    private readonly ITenantContextAccessor _tenantAccessor;
    private readonly ITenantStore _tenantStore;
    private readonly IWebHostEnvironment _env;

    public HomeController(
        ITenantDiscountService discountService,
        ITenantContextAccessor tenantAccessor,
        ITenantStore tenantStore,
        IWebHostEnvironment env)
    {
        _discountService = discountService;
        _tenantAccessor = tenantAccessor;
        _tenantStore = tenantStore;
        _env = env;
    }

    public IActionResult Index(decimal? amount)
    {
        var tenantContext = _tenantAccessor.TenantContext!;
        var currentTenant = tenantContext.CurrentTenant;

        // Calculate sample discount using tenant-resolved C# service
        var testAmount = amount ?? 1000m;
        var discountResult = _discountService.CalculateDiscount(testAmount);

        // Detect if a tenant-specific .js override exists on disk
        var tenantJsPath = $"tenants/{currentTenant.Id}/js/site.js";
        var hasCustomJs = _env.WebRootFileProvider.GetFileInfo(tenantJsPath).Exists;

        // Detect if a tenant-specific .cshtml view override exists on disk
        var tenantCshtmlPath = Path.Combine(_env.ContentRootPath, "Tenants", currentTenant.Id, "Views", "Home", "Index.cshtml");
        var hasCustomCshtml = System.IO.File.Exists(tenantCshtmlPath);

        var viewModel = new HomeViewModel
        {
            CurrentTenant = currentTenant,
            ResolutionSource = tenantContext.ResolutionSource,
            Discount = discountResult,
            RenderedViewSource = hasCustomCshtml
                ? $"Tenants/{currentTenant.Id}/Views/Home/Index.cshtml (Custom Tenant Override)"
                : "Views/Home/Index.cshtml (Base Global Fallback)",
            ScriptFileExpected = hasCustomJs
                ? $"/tenants/{currentTenant.Id}/js/site.js (Custom Tenant Override)"
                : "/js/site.js (Base Global Fallback)",
            ScriptIsOverridden = hasCustomJs,
            AvailableTenants = _tenantStore.GetAllTenants()
        };

        return View(viewModel);
    }

    [HttpGet("switch/{tenantId}")]
    public IActionResult Switch(string tenantId, string? returnUrl)
    {
        var tenant = _tenantStore.GetTenant(tenantId);
        if (tenant != null)
        {
            Response.Cookies.Append("X-Selected-Tenant", tenant.Id, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index), new { tenant = tenant?.Id });
    }

    public IActionResult Privacy()
    {
        var tenantContext = _tenantAccessor.TenantContext!;
        ViewData["TenantName"] = tenantContext.CurrentTenant.Name;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
