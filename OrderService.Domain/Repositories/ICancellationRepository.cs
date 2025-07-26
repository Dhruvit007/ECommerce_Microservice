using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface ICancellationRepository
    {
        // Get a cancellation by its unique ID (Customer or Admin).
        Task<Cancellation?> GetByIdAsync(Guid cancellationId);

        // Get list of cancellations for a specific order (Customer/My Orders).
        Task<List<Cancellation>> GetByOrderIdAsync(Guid orderId);

        // Add a new cancellation request (Customer).
        Task<Cancellation?> AddAsync(Cancellation cancellation);

        // Update an existing cancellation (Customer can update reason before admin review).
        Task<Cancellation?> UpdateAsync(Cancellation cancellation);

        // Delete a cancellation by ID (Customer can request, Admin can approve).
        Task DeleteAsync(Guid cancellationId);

        // Approve a cancellation request (Admin).
        Task ApproveAsync(Guid cancellationId, string approvedBy, decimal refundableAmount, string? remarks = null);

        // Reject a cancellation request (Admin).
        Task RejectAsync(Guid cancellationId, string rejectedBy, string? remarks = null);

        // Get all cancellation items linked to a cancellation (Admin or Customer).
        Task<List<CancellationItem>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId);

        // Check if a cancellation exists by ID.
        Task<bool> ExistsAsync(Guid cancellationId);

        // Get paginated list of all cancellations (Admin, Reporting).
        Task<List<Cancellation>> GetAllAsync(int pageNumber = 1, int pageSize = 20);

        // Get cancellations by status (Admin, Reporting).
        Task<List<Cancellation>> GetByStatusAsync(CancellationStatusEnum cancellationStatusId, int pageNumber = 1, int pageSize = 20);

        // Get cancellations for a user (Customer).
        Task<List<Cancellation>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20);

        // Get cancellations by date range (Admin, Reporting).
        Task<List<Cancellation>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20);
    }
}
