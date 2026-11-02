namespace MultiTenantMvc.Services;

public record DiscountResult(
    decimal OriginalAmount,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal FinalAmount,
    string TierName,
    string ServiceClass,
    string CustomNotes
);

public interface ITenantDiscountService
{
    DiscountResult CalculateDiscount(decimal originalAmount);
}
