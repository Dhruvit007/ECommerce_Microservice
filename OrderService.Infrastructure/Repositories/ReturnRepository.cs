using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly OrderDbContext _dbContext;

        public ReturnRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Get a return request by its unique ID (Customer or Admin)
        public async Task<Return?> GetByIdAsync(Guid returnId)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnItems)
                .Include(r => r.ReturnStatus)
                .Include(r => r.Reason)
                .Include(r => r.Policy)
                .FirstOrDefaultAsync(r => r.Id == returnId && !r.IsDeleted);
        }

        // Get all returns for a specific order (Customer/My Orders)
        public async Task<List<Return>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnItems)
                .Include(r => r.ReturnStatus)
                .Include(r => r.Reason)
                .Include(r => r.Policy)
                .Where(r => r.OrderId == orderId && !r.IsDeleted)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        // Add a new return request (Customer)
        public async Task<Return?> AddAsync(Return returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            returnRequest.RequestedAt = DateTime.UtcNow;
            returnRequest.IsDeleted = false;
            returnRequest.ReturnStatusId = ReturnStatusEnum.Pending;

            await _dbContext.Returns.AddAsync(returnRequest);
            await _dbContext.SaveChangesAsync();

            return returnRequest;
        }

        // Update an existing return (Customer can update reason before admin review)
        public async Task<Return?> UpdateAsync(Return returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            var existing = await _dbContext.Returns
                .Include(r => r.ReturnItems)
                .FirstOrDefaultAsync(r => r.Id == returnRequest.Id && !r.IsDeleted);

            if (existing == null)
                return null;

            // Only allow update if status is Requested (Pending)
            if (existing.ReturnStatusId != ReturnStatusEnum.Pending)
                return null;

            // Update allowed fields
            existing.Remarks = returnRequest.Remarks;
            existing.ReasonId = returnRequest.ReasonId;
            existing.IsPartial = returnRequest.IsPartial;
            existing.ReturnPolicyId = returnRequest.ReturnPolicyId;

            // Synchronize ReturnItems
            var incomingItemIds = returnRequest.ReturnItems?.Select(i => i.Id).ToHashSet() ?? new HashSet<Guid>();

            // Remove items that no longer exist
            var itemsToRemove = existing.ReturnItems.Where(ei => !incomingItemIds.Contains(ei.Id)).ToList();
            _dbContext.ReturnItems.RemoveRange(itemsToRemove);

            // Update or add items
            if (returnRequest.ReturnItems != null)
            {
                foreach (var incomingItem in returnRequest.ReturnItems)
                {
                    var existingItem = existing.ReturnItems.FirstOrDefault(ei => ei.Id == incomingItem.Id);
                    if (existingItem != null)
                    {
                        existingItem.ReturnedQuantity = incomingItem.ReturnedQuantity;
                        existingItem.OrderItemId = incomingItem.OrderItemId;
                        existingItem.Remarks = incomingItem.Remarks;
                    }
                    else
                    {
                        incomingItem.ReturnId = existing.Id;
                        _dbContext.ReturnItems.Add(incomingItem);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        // Delete (soft delete) a return request by ID
        public async Task DeleteAsync(Guid returnId)
        {
            var returnRequest = await _dbContext.Returns.FindAsync(returnId);
            if (returnRequest == null)
                return;

            // Soft delete
            returnRequest.IsDeleted = true;
            returnRequest.DeletedAt = DateTime.UtcNow;

            _dbContext.Returns.Update(returnRequest);
            await _dbContext.SaveChangesAsync();
        }

        // Approve a return request (Admin)
        // Refundable amount is passed from the application layer
        public async Task ApproveAsync(Guid returnId, string approvedBy, decimal refundableAmount, string? remarks = null)
        {
            var returnRequest = await _dbContext.Returns.FindAsync(returnId);
            if (returnRequest == null)
                throw new KeyNotFoundException("Return request not found.");

            if (returnRequest.ReturnStatusId != ReturnStatusEnum.Pending)
                throw new InvalidOperationException("Return request cannot be approved in its current state.");

            returnRequest.ReturnStatusId = ReturnStatusEnum.Approved;
            returnRequest.ApprovedBy = approvedBy;
            returnRequest.ApprovalRemarks = remarks;
            returnRequest.ApprovedAt = DateTime.UtcNow;
            returnRequest.ProcessedBy = approvedBy;
            returnRequest.ProcessedAt = returnRequest.ApprovedAt;
            returnRequest.TotalRefundableAmount = refundableAmount;

            _dbContext.Returns.Update(returnRequest);
            await _dbContext.SaveChangesAsync();
        }

        // Reject a return request (Admin)
        public async Task RejectAsync(Guid returnId, string rejectedBy, string? remarks = null)
        {
            var returnRequest = await _dbContext.Returns.FindAsync(returnId);
            if (returnRequest == null)
                throw new KeyNotFoundException("Return request not found.");

            if (returnRequest.ReturnStatusId != ReturnStatusEnum.Pending)
                throw new InvalidOperationException("Return request cannot be rejected in its current state.");

            returnRequest.ReturnStatusId = ReturnStatusEnum.Rejected;
            returnRequest.RejectedBy = rejectedBy;
            returnRequest.RejectionRemarks = remarks;
            returnRequest.RejectedAt = DateTime.UtcNow;
            returnRequest.ProcessedBy = rejectedBy;
            returnRequest.ProcessedAt = returnRequest.RejectedAt;

            _dbContext.Returns.Update(returnRequest);
            await _dbContext.SaveChangesAsync();
        }

        // Get all return items linked to a return (Admin or Customer)
        public async Task<List<ReturnItem>> GetReturnItemsByReturnIdAsync(Guid returnId)
        {
            return await _dbContext.ReturnItems
                .AsNoTracking()
                .Where(ri => ri.ReturnId == returnId)
                .ToListAsync();
        }

        // Check if a return exists by ID
        public async Task<bool> ExistsAsync(Guid returnId)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .AnyAsync(r => r.Id == returnId && !r.IsDeleted);
        }

        // Get paginated list of all returns (Admin, Reporting)
        public async Task<List<Return>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnStatus)
                .Include(r => r.Policy)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.ReturnStatus)
                .ToListAsync();
        }

        // Get returns by status (Admin, Reporting)
        public async Task<List<Return>> GetByStatusAsync(ReturnStatusEnum returnStatusId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnStatus)
                .Include(r => r.Policy)
                .Where(r => r.ReturnStatusId == returnStatusId && !r.IsDeleted)
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get returns for a user (Customer)
        public async Task<List<Return>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnStatus)
                .Include(r => r.Policy)
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get returns by date range (Admin, Reporting)
        public async Task<List<Return>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Returns
                .AsNoTracking()
                .Include(r => r.ReturnStatus)
                .Include(r => r.Policy)
                .Where(r => r.RequestedAt >= fromDate && r.RequestedAt <= toDate && !r.IsDeleted)
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
