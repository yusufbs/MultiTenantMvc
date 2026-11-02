using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Razor;
using MultiTenantMvc.Infrastructure.Multitenancy;
using MultiTenantMvc.Infrastructure.ViewEngine;
using MultiTenantMvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure local keys directory for DataProtection
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, ".keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// 1. Add MVC Controllers and configure custom Tenant View Location Expander (.cshtml overrides)
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
    });

// 2. Add Multi-Tenancy Core Infrastructure (Context, Store, Accessor)
builder.Services.AddMultiTenancy();

// 3. Register Tenant-Specific C# Backend Services (.cs overrides using Keyed Services)
// Base global implementation
builder.Services.AddKeyedScoped<ITenantDiscountService, DefaultDiscountService>("base");

// Tenant Alpha custom implementation override
builder.Services.AddKeyedScoped<ITenantDiscountService, AlphaDiscountService>("alpha");

// Dynamic resolver: resolves tenant-specific service if registered, otherwise falls back to base
builder.Services.AddScoped<ITenantDiscountService>(sp =>
{
    var tenant = sp.GetRequiredService<TenantInfo>();
    var keyedService = sp.GetKeyedService<ITenantDiscountService>(tenant.Id);
    return keyedService ?? sp.GetRequiredKeyedService<ITenantDiscountService>("base");
});

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. Resolve Tenant Context for every incoming request
app.UseMultiTenancy();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
