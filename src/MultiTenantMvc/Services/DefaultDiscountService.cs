namespace MultiTenantMvc.Services;

public class DefaultDiscountService : ITenantDiscountService
{
    public DiscountResult CalculateDiscount(decimal originalAmount)
    {
        const decimal discountRate = 0.05m; // 5% default platform discount
        var discountAmount = Math.Round(originalAmount * discountRate, 2);
        var finalAmount = originalAmount - discountAmount;

        return new DiscountResult(
            OriginalAmount: originalAmount,
            DiscountPercentage: 5.0m,
            DiscountAmount: discountAmount,
            FinalAmount: finalAmount,
            TierName: "Base Platform Standard Tier",
            ServiceClass: nameof(DefaultDiscountService),
            CustomNotes: "Standard 5% discount applied via default global C# logic (DefaultDiscountService.cs). No custom backend overrides."
        );
    }
}
