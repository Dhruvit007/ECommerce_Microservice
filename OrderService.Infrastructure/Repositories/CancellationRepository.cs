using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class CancellationRepository : ICancellationRepository
    {
        private readonly OrderDbContext _dbContext;

        public CancellationRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Get a cancellation by its unique ID (Customer or Admin)
        public async Task<Cancellation?> GetByIdAsync(Guid cancellationId)
        {
            // Loads all relevant navigation properties for full details (read-only)
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationItems)
                .Include(c => c.CancellationStatus)
                .Include(c => c.Reason)
                .Include(c => c.Policy)
                .FirstOrDefaultAsync(c => c.Id == cancellationId && !c.IsDeleted);
        }

        // Get list of cancellations for a specific order (Customer/My Orders)
        public async Task<List<Cancellation>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationItems)
                .Include(c => c.CancellationStatus)
                .Include(c => c.Reason)
                .Include(c => c.Policy)
                .Where(c => c.OrderId == orderId && !c.IsDeleted)
                .OrderByDescending(c => c.RequestedAt)
                .ToListAsync();
        }

        // Add a new cancellation request (Customer)
        public async Task<Cancellation?> AddAsync(Cancellation cancellation)
        {
            if (cancellation == null)
                throw new ArgumentNullException(nameof(cancellation));

            cancellation.RequestedAt = DateTime.UtcNow;
            cancellation.IsDeleted = false;
            cancellation.CancellationStatusId = CancellationStatusEnum.Pending;

            await _dbContext.Cancellations.AddAsync(cancellation);
            await _dbContext.SaveChangesAsync();
            return cancellation;
        }

        // Update an existing cancellation (Customer can update before admin review)
        public async Task<Cancellation?> UpdateAsync(Cancellation cancellation)
        {
            if (cancellation == null)
                throw new ArgumentNullException(nameof(cancellation));

            var existing = await _dbContext.Cancellations
                .Include(c => c.CancellationItems)
                .FirstOrDefaultAsync(c => c.Id == cancellation.Id && !c.IsDeleted);

            if (existing == null)
                return null;

            // Only allow update if status is Pending (Requested)
            if (existing.CancellationStatusId != CancellationStatusEnum.Pending)
            {
                // Option 1: Return null to indicate update not allowed
                return null;

                // Option 2: throw new InvalidOperationException("Cannot update cancellation in current status");
            }

            // Update allowed fields
            existing.Remarks = cancellation.Remarks;
            existing.ReasonId = cancellation.ReasonId;
            existing.IsPartial = cancellation.IsPartial;
            existing.CancellationPolicyId = cancellation.CancellationPolicyId;

            // Synchronize CancellationItems

            // Remove items that are no longer present
            var incomingItemIds = cancellation.CancellationItems?.Select(i => i.Id).ToHashSet() ?? new HashSet<Guid>();
            var itemsToRemove = existing.CancellationItems.Where(ei => !incomingItemIds.Contains(ei.Id)).ToList();
            _dbContext.CancellationItems.RemoveRange(itemsToRemove);

            // Update or add items
            if (cancellation.CancellationItems != null)
            {
                foreach (var incomingItem in cancellation.CancellationItems)
                {
                    var existingItem = existing.CancellationItems.FirstOrDefault(ei => ei.Id == incomingItem.Id);
                    if (existingItem != null)
                    {
                        // Update existing item fields
                        existingItem.CancelledQuantity = incomingItem.CancelledQuantity;
                        existingItem.OrderItemId = incomingItem.OrderItemId;
                    }
                    else
                    {
                        // New item: attach to existing cancellation
                        incomingItem.CancellationId = existing.Id;
                        _dbContext.CancellationItems.Add(incomingItem);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        // Delete (soft delete) a cancellation by ID (Customer request)
        public async Task DeleteAsync(Guid cancellationId)
        {
            var cancellation = await _dbContext.Cancellations.FindAsync(cancellationId);
            if (cancellation == null) return;

            // Only allow delete if status is Requested (Pending)
            if (cancellation.CancellationStatusId != CancellationStatusEnum.Pending)
            {
                // Option 1: silently return
                // return;

                // Option 2: throw exception to notify caller
                throw new InvalidOperationException("Cannot delete cancellation that is already processed.");
            }

            // Perform soft delete
            cancellation.IsDeleted = true;
            cancellation.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        // Approve a cancellation request (Admin)
        public async Task ApproveAsync(Guid cancellationId, string approvedBy, decimal refundableAmount, string? remarks = null)
        {
            var cancellation = await _dbContext.Cancellations.FindAsync(cancellationId);
            if (cancellation == null)
                throw new KeyNotFoundException("Cancellation not found.");

            // Only allow approving if current status is Requested (Pending)
            if (cancellation.CancellationStatusId != CancellationStatusEnum.Pending)
                throw new InvalidOperationException("Cancellation cannot be approved in its current state.");

            // Update status and audit info
            cancellation.CancellationStatusId = CancellationStatusEnum.Approved;
            cancellation.ApprovedBy = approvedBy;
            cancellation.ApprovalRemarks = remarks;
            cancellation.ApprovedAt = DateTime.UtcNow;
            cancellation.ProcessedBy = approvedBy;
            cancellation.ProcessedAt = cancellation.ApprovedAt;

            // Set the refundable amount as provided
            cancellation.TotalRefundableAmount = refundableAmount;

            _dbContext.Cancellations.Update(cancellation);
            await _dbContext.SaveChangesAsync();
        }

        // Reject a cancellation request (Admin)
        public async Task RejectAsync(Guid cancellationId, string rejectedBy, string? remarks = null)
        {
            var cancellation = await _dbContext.Cancellations.FindAsync(cancellationId);
            if (cancellation == null)
                throw new KeyNotFoundException("Cancellation not found.");

            // Only allow rejecting if current status is Requested (Pending)
            if (cancellation.CancellationStatusId != CancellationStatusEnum.Pending)
                throw new InvalidOperationException("Cancellation cannot be rejected in its current state.");

            // Update status and audit info
            cancellation.CancellationStatusId = CancellationStatusEnum.Rejected;
            cancellation.RejectedBy = rejectedBy;
            cancellation.RejectionRemarks = remarks;
            cancellation.RejectedAt = DateTime.UtcNow;
            cancellation.ProcessedBy = rejectedBy;
            cancellation.ProcessedAt = cancellation.RejectedAt;

            _dbContext.Cancellations.Update(cancellation);
            await _dbContext.SaveChangesAsync();
        }

        // Get all cancellation items linked to a cancellation (Admin or Customer)
        public async Task<List<CancellationItem>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId)
        {
            return await _dbContext.CancellationItems
                .AsNoTracking()
                .Where(ci => ci.CancellationId == cancellationId)
                .ToListAsync();
        }

        // Check if a cancellation exists by ID
        public async Task<bool> ExistsAsync(Guid cancellationId)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .AnyAsync(c => c.Id == cancellationId && !c.IsDeleted);
        }

        // Get paginated list of all cancellations (Admin, Reporting)
        public async Task<List<Cancellation>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationStatus)
                .Include(c => c.Policy)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get cancellations by status (Admin, Reporting)
        public async Task<List<Cancellation>> GetByStatusAsync(CancellationStatusEnum cancellationStatusId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationStatus)
                .Include(c => c.Policy)
                .Where(c => c.CancellationStatusId == cancellationStatusId && !c.IsDeleted)
                .OrderByDescending(c => c.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get cancellations for a user (Customer)
        public async Task<List<Cancellation>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationStatus)
                .Include(c => c.Policy)
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get cancellations by date range (Admin, Reporting)
        public async Task<List<Cancellation>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Cancellations
                .AsNoTracking()
                .Include(c => c.CancellationStatus)
                .Include(c => c.Policy)
                .Where(c => c.RequestedAt >= fromDate && c.RequestedAt <= toDate && !c.IsDeleted)
                .OrderByDescending(c => c.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
