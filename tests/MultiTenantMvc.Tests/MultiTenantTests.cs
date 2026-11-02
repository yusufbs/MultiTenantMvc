using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MultiTenantMvc.Infrastructure.Multitenancy;
using MultiTenantMvc.Infrastructure.TagHelpers;
using MultiTenantMvc.Infrastructure.ViewEngine;
using MultiTenantMvc.Services;
using Xunit;

namespace MultiTenantMvc.Tests;

public class MultiTenantTests
{
    public static async Task<int> Main()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine(" RUNNING MULTI-TENANT SOLUTION VERIFICATION SUITE");
        Console.WriteLine("=================================================");

        var tests = new MultiTenantTests();
        int passed = 0;

        void Run(string name, Action act)
        {
            try
            {
                act();
                Console.WriteLine($" [PASS] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [FAIL] {name}: {ex.Message}");
            }
        }

        async Task RunAsync(string name, Func<Task> act)
        {
            try
            {
                await act();
                Console.WriteLine($" [PASS] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [FAIL] {name}: {ex.Message}");
            }
        }

        Run("TenantStore contains Base and both Tenants (Alpha, Beta)", () => tests.TenantStore_ContainsBaseAndBothTenants());
        Run("TenantStore resolves identifiers case-insensitively", () =>
        {
            tests.TenantStore_ResolvesIdentifiersCorrectly("alpha", "alpha");
            tests.TenantStore_ResolvesIdentifiersCorrectly("ALPHA", "alpha");
            tests.TenantStore_ResolvesIdentifiersCorrectly("beta", "beta");
            tests.TenantStore_ResolvesIdentifiersCorrectly("base", "base");
        });
        await RunAsync("TenantResolutionMiddleware resolves via Query String (?tenant=alpha)", () => tests.TenantResolutionMiddleware_ResolvesViaQueryString());
        await RunAsync("TenantResolutionMiddleware resolves via HTTP Header (X-Tenant: beta)", () => tests.TenantResolutionMiddleware_ResolvesViaHeader());
        await RunAsync("TenantResolutionMiddleware falls back to Global Base default", () => tests.TenantResolutionMiddleware_FallsBackToBaseWhenNoMatch());
        Run("Keyed DI resolves custom C# service (AlphaDiscountService) for Tenant Alpha", () => tests.DependencyInjection_ResolvesCustomCsServiceForAlpha());
        Run("Keyed DI falls back to base C# service (DefaultDiscountService) for Tenant Beta", () => tests.DependencyInjection_FallsBackToBaseCsServiceForBeta());
        Run("TenantViewLocationExpander prepends /Tenants/{id}/Views/ for Razor resolution", () => tests.ViewLocationExpander_ExpandsTenantSpecificPaths());
        Run("TenantScriptTagHelper resolves /tenants/beta/js/site.js override for Tenant Beta", () => tests.TenantScriptTagHelper_ResolvesBetaCustomScript_WhenPresent());
        Run("TenantScriptTagHelper falls back to /js/site.js for Tenant Alpha (no override)", () => tests.TenantScriptTagHelper_FallsBackToBaseScript_WhenTenantOverrideMissing());

        Console.WriteLine("=================================================");
        Console.WriteLine($" RESULT: {passed} / 10 Tests Passed Successfully!");
        Console.WriteLine("=================================================");
        return passed == 10 ? 0 : 1;
    }
    [Fact]
    public void TenantStore_ContainsBaseAndBothTenants()
    {
        var store = new InMemoryTenantStore();

        var baseTenant = store.GetTenant("base");
        var alphaTenant = store.GetTenant("alpha");
        var betaTenant = store.GetTenant("beta");
        var defaultTenant = store.GetDefaultTenant();

        Assert.NotNull(baseTenant);
        Assert.NotNull(alphaTenant);
        Assert.NotNull(betaTenant);
        Assert.Equal("base", defaultTenant.Id);
        Assert.Equal("Tenant Alpha (Enterprise Solutions)", alphaTenant.Name);
        Assert.Equal("Tenant Beta (Retail Network)", betaTenant.Name);
    }

    [Theory]
    [InlineData("alpha", "alpha")]
    [InlineData("ALPHA", "alpha")]
    [InlineData("beta", "beta")]
    [InlineData("base", "base")]
    [InlineData("unknown", null)]
    public void TenantStore_ResolvesIdentifiersCorrectly(string input, string? expectedId)
    {
        var store = new InMemoryTenantStore();
        var tenant = store.GetTenant(input);

        Assert.Equal(expectedId, tenant?.Id);
    }

    [Fact]
    public async Task TenantResolutionMiddleware_ResolvesViaQueryString()
    {
        var store = new InMemoryTenantStore();
        var context = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };
        var accessor = new TenantContextAccessor(httpContextAccessor);
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        context.Request.QueryString = new QueryString("?tenant=alpha");

        await middleware.InvokeAsync(context, store, accessor);

        Assert.NotNull(accessor.TenantContext);
        Assert.Equal("alpha", accessor.TenantContext.CurrentTenant.Id);
        Assert.Contains("Query Parameter", accessor.TenantContext.ResolutionSource);
    }

    [Fact]
    public async Task TenantResolutionMiddleware_ResolvesViaHeader()
    {
        var store = new InMemoryTenantStore();
        var context = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };
        var accessor = new TenantContextAccessor(httpContextAccessor);
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        context.Request.Headers["X-Tenant"] = "beta";

        await middleware.InvokeAsync(context, store, accessor);

        Assert.NotNull(accessor.TenantContext);
        Assert.Equal("beta", accessor.TenantContext.CurrentTenant.Id);
        Assert.Contains("HTTP Header", accessor.TenantContext.ResolutionSource);
    }

    [Fact]
    public async Task TenantResolutionMiddleware_FallsBackToBaseWhenNoMatch()
    {
        var store = new InMemoryTenantStore();
        var context = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };
        var accessor = new TenantContextAccessor(httpContextAccessor);
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, store, accessor);

        Assert.NotNull(accessor.TenantContext);
        Assert.Equal("base", accessor.TenantContext.CurrentTenant.Id);
        Assert.Equal("Global Base Fallback", accessor.TenantContext.ResolutionSource);
    }

    [Fact]
    public void DependencyInjection_ResolvesCustomCsServiceForAlpha()
    {
        // 1. Arrange DI container as configured in Program.cs
        var services = new ServiceCollection();
        services.AddMultiTenancy();

        // Register keyed services
        services.AddKeyedScoped<ITenantDiscountService, DefaultDiscountService>("base");
        services.AddKeyedScoped<ITenantDiscountService, AlphaDiscountService>("alpha");

        services.AddScoped<ITenantDiscountService>(sp =>
        {
            var tenant = sp.GetRequiredService<TenantInfo>();
            var keyed = sp.GetKeyedService<ITenantDiscountService>(tenant.Id);
            return keyed ?? sp.GetRequiredKeyedService<ITenantDiscountService>("base");
        });

        // 2. Simulate Tenant Alpha request
        var spAlpha = services.BuildServiceProvider().CreateScope().ServiceProvider;
        var accessor = spAlpha.GetRequiredService<ITenantContextAccessor>();
        var store = spAlpha.GetRequiredService<ITenantStore>();
        accessor.TenantContext = new TenantContext
        {
            CurrentTenant = store.GetTenant("alpha")!,
            ResolutionSource = "Unit Test"
        };

        var discountService = spAlpha.GetRequiredService<ITenantDiscountService>();
        var result = discountService.CalculateDiscount(1000m);

        // Assert: Tenant Alpha should resolve AlphaDiscountService and apply 30% volume discount
        Assert.IsType<AlphaDiscountService>(discountService);
        Assert.Equal(30.0m, result.DiscountPercentage);
        Assert.Equal(300m, result.DiscountAmount);
        Assert.Equal(700m, result.FinalAmount);
        Assert.Equal("AlphaDiscountService", result.ServiceClass);
    }

    [Fact]
    public void DependencyInjection_FallsBackToBaseCsServiceForBeta()
    {
        // 1. Arrange DI container
        var services = new ServiceCollection();
        services.AddMultiTenancy();
        services.AddKeyedScoped<ITenantDiscountService, DefaultDiscountService>("base");
        services.AddKeyedScoped<ITenantDiscountService, AlphaDiscountService>("alpha");

        services.AddScoped<ITenantDiscountService>(sp =>
        {
            var tenant = sp.GetRequiredService<TenantInfo>();
            var keyed = sp.GetKeyedService<ITenantDiscountService>(tenant.Id);
            return keyed ?? sp.GetRequiredKeyedService<ITenantDiscountService>("base");
        });

        // 2. Simulate Tenant Beta request (no custom .cs service registered)
        var spBeta = services.BuildServiceProvider().CreateScope().ServiceProvider;
        var accessor = spBeta.GetRequiredService<ITenantContextAccessor>();
        var store = spBeta.GetRequiredService<ITenantStore>();
        accessor.TenantContext = new TenantContext
        {
            CurrentTenant = store.GetTenant("beta")!,
            ResolutionSource = "Unit Test"
        };

        var discountService = spBeta.GetRequiredService<ITenantDiscountService>();
        var result = discountService.CalculateDiscount(1000m);

        // Assert: Tenant Beta seamlessly falls back to DefaultDiscountService (5% discount)
        Assert.IsType<DefaultDiscountService>(discountService);
        Assert.Equal(5.0m, result.DiscountPercentage);
        Assert.Equal(50m, result.DiscountAmount);
        Assert.Equal(950m, result.FinalAmount);
        Assert.Equal("DefaultDiscountService", result.ServiceClass);
    }

    [Fact]
    public void ViewLocationExpander_ExpandsTenantSpecificPaths()
    {
        var expander = new TenantViewLocationExpander();
        var httpContext = new DefaultHttpContext();
        httpContext.Items["TenantContext"] = new TenantContext
        {
            CurrentTenant = new InMemoryTenantStore().GetTenant("alpha")!,
            ResolutionSource = "Unit Test"
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var context = new ViewLocationExpanderContext(actionContext, "Index", "Home", areaName: null, pageName: null, isMainPage: true);

        expander.PopulateValues(context);
        Assert.Equal("alpha", context.Values["tenant"]);

        var baseLocations = new[] { "/Views/{1}/{0}.cshtml", "/Views/Shared/{0}.cshtml" };
        var expanded = expander.ExpandViewLocations(context, baseLocations).ToList();

        // Must prepend tenant folders before base locations
        Assert.Equal("/Tenants/alpha/Views/{1}/{0}.cshtml", expanded[0]);
        Assert.Equal("/Tenants/alpha/Views/Shared/{0}.cshtml", expanded[1]);
        Assert.Equal("/Views/{1}/{0}.cshtml", expanded[2]);
        Assert.Equal("/Views/Shared/{0}.cshtml", expanded[3]);
    }

    [Fact]
    public void TenantScriptTagHelper_ResolvesBetaCustomScript_WhenPresent()
    {
        var store = new InMemoryTenantStore();
        var accessor = new TenantContextAccessor
        {
            TenantContext = new TenantContext
            {
                CurrentTenant = store.GetTenant("beta")!,
                ResolutionSource = "Unit Test"
            }
        };

        // Create mock file provider that reports /tenants/beta/js/site.js exists
        var env = new TestWebHostEnvironment();
        env.AddExistingFile("tenants/beta/js/site.js");

        var tagHelper = new TenantScriptTagHelper(env, accessor)
        {
            Src = "site.js"
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "tenant-script",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        Assert.Equal("script", output.TagName);
        Assert.Equal("/tenants/beta/js/site.js", output.Attributes["src"].Value);
        Assert.Equal("beta", output.Attributes["data-tenant"].Value);
        Assert.Equal("true", output.Attributes["data-is-tenant-override"].Value);
    }

    [Fact]
    public void TenantScriptTagHelper_FallsBackToBaseScript_WhenTenantOverrideMissing()
    {
        var store = new InMemoryTenantStore();
        var accessor = new TenantContextAccessor
        {
            TenantContext = new TenantContext
            {
                CurrentTenant = store.GetTenant("alpha")!, // Alpha has no custom script
                ResolutionSource = "Unit Test"
            }
        };

        var env = new TestWebHostEnvironment(); // No custom file added for alpha

        var tagHelper = new TenantScriptTagHelper(env, accessor)
        {
            Src = "site.js"
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "tenant-script",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        Assert.Equal("script", output.TagName);
        Assert.Equal("/js/site.js", output.Attributes["src"].Value);
        Assert.Equal("alpha", output.Attributes["data-tenant"].Value);
        Assert.Equal("false", output.Attributes["data-is-tenant-override"].Value);
    }
}

// Minimal Test Environment Helper for TagHelper testing
internal class TestWebHostEnvironment : IWebHostEnvironment
{
    private readonly HashSet<string> _existingFiles = new(StringComparer.OrdinalIgnoreCase);
    private IFileProvider? _webRootFileProvider;

    public void AddExistingFile(string path) => _existingFiles.Add(path.Replace('\\', '/'));

    public IFileProvider WebRootFileProvider
    {
        get => _webRootFileProvider ?? new TestFileProvider(_existingFiles);
        set => _webRootFileProvider = value;
    }
    public string WebRootPath { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = "MultiTenantMvc";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = "Development";
}

internal class TestFileProvider : IFileProvider
{
    private readonly HashSet<string> _existingFiles;
    public TestFileProvider(HashSet<string> files) => _existingFiles = files;

    public IDirectoryContents GetDirectoryContents(string subpath) => new NotFoundDirectoryContents();

    public IFileInfo GetFileInfo(string subpath)
    {
        var normalized = subpath.TrimStart('/').Replace('\\', '/');
        var exists = _existingFiles.Contains(normalized);
        return new TestFileInfo(exists);
    }

    public Microsoft.Extensions.Primitives.IChangeToken Watch(string filter) => NullChangeToken.Singleton;
}

internal class TestFileInfo : IFileInfo
{
    public TestFileInfo(bool exists) => Exists = exists;
    public bool Exists { get; }
    public long Length => 100;
    public string? PhysicalPath => null;
    public string Name => "file";
    public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
    public bool IsDirectory => false;
    public Stream CreateReadStream() => new MemoryStream();
}
