using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class RefundRepository : IRefundRepository
    {
        private readonly OrderDbContext _dbContext;

        private static readonly Dictionary<RefundStatusEnum, List<RefundStatusEnum>> AllowedTransitions = new()
        {
            // From Pending, can go to Processing or Cancelled
            { RefundStatusEnum.Pending, new List<RefundStatusEnum> { RefundStatusEnum.Processing, RefundStatusEnum.Cancelled } },

            // From Processing, can go to Completed, or Failed, 
            { RefundStatusEnum.Processing, new List<RefundStatusEnum> { RefundStatusEnum.Completed, RefundStatusEnum.Failed} },

            // From Failed, can go to Completed with try
            { RefundStatusEnum.Failed, new List<RefundStatusEnum> { RefundStatusEnum.Completed} },

            // From Cancelled or Completed, no further transitions allowed
            { RefundStatusEnum.Cancelled, new List<RefundStatusEnum>() },
            { RefundStatusEnum.Completed, new List<RefundStatusEnum>() }
        };

        public RefundRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Get refund by ID with related data, no tracking for performance
        public async Task<Refund?> GetByIdAsync(Guid refundId)
        {
            return await _dbContext.Refunds
                .Include(r => r.RefundItems)
                .Include(r => r.RefundStatus)
                .Include(r => r.Order)
                .Include(r => r.Cancellation)
                .Include(r => r.Return)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == refundId);
        }

        // Get refunds for specific order
        public async Task<List<Refund>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Refunds
                .Include(r => r.RefundItems)
                .Include(r => r.RefundStatus)
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.RefundDate)
                .AsNoTracking()
                .ToListAsync();
        }

        // Add a new refund request
        public async Task<Refund?> AddAsync(Refund refund)
        {
            if (refund == null) throw new ArgumentNullException(nameof(refund));

            refund.RefundDate = DateTime.UtcNow;
            refund.IsDeleted = false;
            refund.RefundStatusId = RefundStatusEnum.Pending;

            await _dbContext.Refunds.AddAsync(refund);
            await _dbContext.SaveChangesAsync();

            return refund;
        }

        // Update refund (usually status changes by admin or payment gateway updates)
        public async Task<Refund?> UpdateAsync(Refund refund)
        {
            if (refund == null)
                throw new ArgumentNullException(nameof(refund));

            var existing = await _dbContext.Refunds
                .Include(r => r.RefundStatus)
                .FirstOrDefaultAsync(r => r.Id == refund.Id);

            if (existing == null)
                return null;

            if (existing.RefundStatusId != refund.RefundStatusId)
            {
                var currentStatus = existing.RefundStatusId;
                var newStatus = refund.RefundStatusId;

                if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed)
                    || !allowed.Contains(newStatus))
                {
                    throw new InvalidOperationException($"Transition from {currentStatus} to {newStatus} is not allowed.");
                }

                existing.RefundStatusId = refund.RefundStatusId;

                // Update other updatable fields
                existing.TransactionReference = refund.TransactionReference;
                existing.RefundDate = refund.RefundDate;
                existing.RefundAmount = refund.RefundAmount;
                existing.RefundedTaxAmount = refund.RefundedTaxAmount;
                existing.RefundedDiscountAmount = refund.RefundedDiscountAmount;
                existing.RefundedShippingCharges = refund.RefundedShippingCharges;
                existing.PaymentMethod = refund.PaymentMethod;

                await _dbContext.SaveChangesAsync();
            }

            return existing;
        }

        // Get all refund items for a refund
        public async Task<List<RefundItem>> GetRefundItemsByRefundIdAsync(Guid refundId)
        {
            return await _dbContext.RefundItems
                .AsNoTracking()
                .Where(ri => ri.RefundId == refundId)
                .ToListAsync();
        }

        // Check if refund exists by ID
        public async Task<bool> ExistsAsync(Guid refundId)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .AnyAsync(r => r.Id == refundId && !r.IsDeleted);
        }

        // Paginated list of all refunds (Admin/Reporting)
        public async Task<List<Refund>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .Include(r => r.Order)
                .OrderByDescending(r => r.RefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Refunds filtered by status
        public async Task<List<Refund>> GetByStatusAsync(RefundStatusEnum refundStatusId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .Include(r => r.Order)
                .Where(r => r.RefundStatusId == refundStatusId)
                .OrderByDescending(r => r.RefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Refunds for a specific user
        public async Task<List<Refund>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            // Assuming Refund entity has UserId or related via Order.UserId (adjust as per your model)
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .Include(r => r.Order)
                .Where(r => r.Order != null && r.Order.UserId == userId)
                .OrderByDescending(r => r.RefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Refunds within date range
        public async Task<List<Refund>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Include(r => r.RefundStatus)
                .Include(r => r.Order)
                .Where(r => r.RefundDate >= fromDate && r.RefundDate <= toDate && !r.IsDeleted)
                .OrderByDescending(r => r.RefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
