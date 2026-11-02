namespace MultiTenantMvc.Services;

/// <summary>
/// Tenant-specific backend C# override for Tenant Alpha (Enterprise).
/// Provides specialized enterprise pricing algorithms, larger volume discounts, and VIP SLAs.
/// </summary>
public class AlphaDiscountService : ITenantDiscountService
{
    public DiscountResult CalculateDiscount(decimal originalAmount)
    {
        // Tenant Alpha has custom enterprise volume tiers
        decimal discountRate = originalAmount switch
        {
            >= 1000m => 0.30m, // 30% for high-volume enterprise orders
            >= 500m => 0.25m,  // 25% for medium enterprise orders
            _ => 0.20m        // 20% baseline for Alpha enterprise members
        };

        var discountAmount = Math.Round(originalAmount * discountRate, 2);
        var finalAmount = originalAmount - discountAmount;

        return new DiscountResult(
            OriginalAmount: originalAmount,
            DiscountPercentage: discountRate * 100,
            DiscountAmount: discountAmount,
            FinalAmount: finalAmount,
            TierName: "Alpha Enterprise VIP Tier (Custom .cs)",
            ServiceClass: nameof(AlphaDiscountService),
            CustomNotes: $"Enterprise custom calculation executed via AlphaDiscountService.cs ({discountRate * 100:0}% volume discount + 24/7 dedicated account manager SLA)."
        );
    }
}
