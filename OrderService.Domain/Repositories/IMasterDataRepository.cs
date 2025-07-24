using OrderService.Domain.Entities;
namespace OrderService.Domain.Repositories
{
    public interface IMasterDataRepository
    {
        Task<List<OrderStatus>> GetOrderStatusesAsync();
        Task<List<CancellationStatus>> GetCancellationStatusesAsync();
        Task<List<ReturnStatus>> GetReturnStatusesAsync();
        Task<List<ReasonMaster>> GetReasonsByTypeAsync(string reasonType);
        Task<List<CancellationPolicy>> GetCancellationPoliciesAsync();
        Task<List<ReturnPolicy>> GetReturnPoliciesAsync();
        Task<List<Discount>> GetDiscountsAsync();
        Task<List<Discount>> GetActiveDiscountsAsync(DateTime asOfDate);
        Task<List<Tax>> GetTaxesAsync();
        Task<List<Tax>> GetActiveTaxesAsync(DateTime asOfDate);
    }
}
