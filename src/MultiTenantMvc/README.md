# Multi-Tenant ASP.NET Core MVC Solution (.NET 10)

A multi-tenant architecture built on **ASP.NET Core (.NET 10)** demonstrating a **Base Platform** and **2 distinct Tenant Sites** (`Tenant Alpha` and `Tenant Beta`) with three hierarchical levels of tenant-specific overrides:

1. **Backend C# Business Logic (`.cs`)** via **Keyed Services & Dependency Injection**
2. **Razor Views (`.cshtml`)** via **`IViewLocationExpander`**
3. **Client-Side Scripts (`.js`)** via **`TenantScriptTagHelper` & Hierarchical Asset Serving**

---

## 🏗️ Architecture & Customization Matrix

| Tenant | C# Backend Logic (`.cs`) | Razor View (`.cshtml`) | Client Script (`.js`) |
| :--- | :--- | :--- | :--- |
| **Base Site (Global)** | `DefaultDiscountService.cs`<br>*(5% standard tier)* | `Views/Home/Index.cshtml`<br>*(Standard base view)* | `wwwroot/js/site.js`<br>*(Core platform JS)* |
| **Tenant Alpha (Enterprise)** | ⭐ **`AlphaDiscountService.cs`**<br>*(20%–30% volume SLA tier)* | ⭐ **`Tenants/alpha/Views/Home/Index.cshtml`**<br>*(Custom enterprise portal layout)* | 🔄 *Falls back to base `/js/site.js`* |
| **Tenant Beta (Retail)** | 🔄 *Falls back to base `DefaultDiscountService.cs`* | 🔄 *Falls back to base `Views/Home/Index.cshtml`* | ⭐ **`wwwroot/tenants/beta/js/site.js`**<br>*(Custom flash promo & retail widget)* |

---

## 📁 Project Directory Structure

```text
MultiTenantMvc (.NET 10)/
├── Controllers/
│   └── HomeController.cs                 # Injects ITenantDiscountService, resolves model & views
├── Infrastructure/
│   ├── Multitenancy/
│   │   ├── TenantInfo.cs                 # Tenant model (Id, Name, Colors, Hostname)
│   │   ├── ITenantStore.cs               # Tenant store contract
│   │   ├── InMemoryTenantStore.cs        # Tenant catalog (base, alpha, beta)
│   │   ├── TenantContext.cs              # Request tenant context & resolution source
│   │   ├── ITenantContextAccessor.cs     # Ambient & HttpContext.Items accessor
│   │   ├── TenantResolutionMiddleware.cs # Resolves tenant (Query, Header, Subdomain, Cookie)
│   │   └── MultitenancyExtensions.cs     # DI registration helpers
│   ├── ViewEngine/
│   │   └── TenantViewLocationExpander.cs # Discovers tenant-specific .cshtml before base
│   └── TagHelpers/
│       └── TenantScriptTagHelper.cs      # Discovers tenant-specific .js before base
├── Services/
│   ├── ITenantDiscountService.cs         # Business logic interface
│   ├── DefaultDiscountService.cs         # Global base implementation (.cs)
│   └── AlphaDiscountService.cs           # Tenant Alpha custom override (.cs)
├── Models/
│   ├── HomeViewModel.cs
│   └── ErrorViewModel.cs
├── Tenants/
│   └── alpha/
│       └── Views/
│           └── Home/
│               └── Index.cshtml          # ⭐ Tenant Alpha Razor View Override (.cshtml)
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml                  # Global Base Razor View
│   │   └── Privacy.cshtml                # Tenant-aware privacy view
│   ├── Shared/
│   │   └── _Layout.cshtml                # Responsive layout with 1-click Tenant Switcher
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── js/
│   │   └── site.js                       # Global Base JavaScript (.js)
│   └── tenants/
│       └── beta/
│           └── js/
│               └── site.js               # ⭐ Tenant Beta JavaScript Override (.js)
├── Program.cs                            # Modern .NET 10 pipeline & keyed DI setup
└── MultiTenantMvc.csproj                 # Target framework net10.0
```

---

## 🚀 How to Run the Solution

1. Open PowerShell or Terminal in the project root:
   ```powershell
   cd "c:\Users\yusuf\OneDrive\Desktop\Prompts\MultiTenantMvc (.NET 10)"
   dotnet run
   ```

2. Open your browser and navigate to:
   - **Base Site:** `http://localhost:5271/?tenant=base`
   - **Tenant Alpha:** `http://localhost:5271/?tenant=alpha`
   - **Tenant Beta:** `http://localhost:5271/?tenant=beta`

*(You can also use the interactive **Multi-Tenant Switcher** toolbar pinned to the top of the webpage to toggle between tenants with one click!)*

---

## 🧪 Verification & Automated Tests

A dedicated test suite `MultiTenantMvc.Tests` validates all multi-tenant behaviors:

```powershell
dotnet "MultiTenantMvc.Tests\bin\Debug\net10.0\MultiTenantMvc.Tests.dll"
```

All 10/10 test scenarios pass:
- ✅ Resolution via query string, header, and fallback
- ✅ Keyed DI custom C# service resolution
- ✅ ViewLocationExpander path expansion
- ✅ TenantScriptTagHelper disk detection and fallback
