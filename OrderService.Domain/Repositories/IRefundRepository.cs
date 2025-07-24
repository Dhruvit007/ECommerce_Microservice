using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories
{
    public interface IRefundRepository
    {
        Task<Refund?> GetByIdAsync(Guid refundId);
        Task<List<Refund>> GetByOrderIdAsync(Guid orderId);
        Task<Refund?> AddAsync(Refund refund);

        // Update refund details (e.g., status, amounts, transaction references)
        Task<Refund?> UpdateAsync(Refund refund);

        // Soft or hard delete a refund record by ID
        Task DeleteAsync(Guid refundId);

        // Get all refund items linked to a particular refund (for item-level tracking)
        Task<List<RefundItem>> GetRefundItemsByRefundIdAsync(Guid refundId);

        // Add a refund item (to track item-level refunded quantities and amounts)
        Task<RefundItem?> AddRefundItemAsync(RefundItem refundItem);
        Task<RefundItem?> UpdateRefundItemAsync(RefundItem refundItem);

        // Delete a refund item by its ID
        Task DeleteRefundItemAsync(Guid refundItemId);

        // Optional: Get refunds filtered by status for reporting or processing
        Task<List<Refund>> GetByRefundStatusAsync(string refundStatus);

        // Optional: Get refunds within a date range for reconciliation
        Task<List<Refund>> GetRefundsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
