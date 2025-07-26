using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;

        // Define allowed transitions dictionary using OrderStatusEnum
        private static readonly Dictionary<OrderStatusEnum, List<OrderStatusEnum>> AllowedTransitions = new()
        {
            //Pending → Confirmed, Cancelled
            { OrderStatusEnum.Pending, new List<OrderStatusEnum> { OrderStatusEnum.Confirmed, OrderStatusEnum.Cancelled } },
            
            //Confirmed → Packed, Cancelled
            { OrderStatusEnum.Confirmed, new List<OrderStatusEnum> { OrderStatusEnum.Packed, OrderStatusEnum.Cancelled } },
            
            //Packed → Shipped, Cancelled
            { OrderStatusEnum.Packed, new List<OrderStatusEnum> { OrderStatusEnum.Shipped, OrderStatusEnum.Cancelled } },
            
            //Shipped → Delivered, Cancelled
            { OrderStatusEnum.Shipped, new List<OrderStatusEnum> { OrderStatusEnum.Delivered, OrderStatusEnum.Cancelled } },
            
            //Delivered → Returned
            { OrderStatusEnum.Delivered, new List<OrderStatusEnum> { OrderStatusEnum.Returned } },  
            
            //Cancelled & Returned → terminal
            { OrderStatusEnum.Cancelled, new List<OrderStatusEnum>() },  // Terminal state
            { OrderStatusEnum.Returned, new List<OrderStatusEnum>() }    // Terminal state
        };

        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Get a specific order by its unique ID (Customer or Admin)
        public async Task<Order?> GetByIdAsync(Guid orderId)
        {
            return await _dbContext.Orders.FindAsync(orderId);
        }

        // Get paginated list of orders for a specific user (most recent first) [Customer]
        public async Task<List<Order>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _dbContext.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Add a new order (Customer)
        public async Task<Order?> AddAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
            return order;
        }

        // Changes the status of the order with optional remarks, also records status history
        public async Task<bool> ChangeOrderStatusAsync(Guid orderId, OrderStatusEnum newStatusId, string? changedBy = null, string? remarks = null)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderStatusHistories)  // Include navigation for history
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            if (order.OrderStatusId == newStatusId)
                return true;

            var oldStatusId = order.OrderStatusId;

            var oldStatus = oldStatusId;
            var newStatus = newStatusId;

            // Validate transition
            if (!AllowedTransitions.TryGetValue(oldStatus, out var validNextStatuses) || !validNextStatuses.Contains(newStatus))
            {
                throw new InvalidOperationException($"Transition from {oldStatus} to {newStatus} is not allowed.");
            }

            // Update the order status
            order.OrderStatusId = newStatusId;

            // Record status history
            order.OrderStatusHistories.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OldStatusId = (int)oldStatusId,
                NewStatusId = (int)newStatusId,
                ChangedBy = changedBy,
                Remarks = remarks,
                ChangedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();

            return true;
        }

        // Gets the full status history for an order (Customer, Admin, Reporting)
        public async Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(Guid orderId)
        {
            return await _dbContext.OrderStatusHistories
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
        }

        // Delete an order by ID (Admin only)
        public async Task DeleteAsync(Guid orderId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null) return;

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
        }

        // Check if an order exists by ID (internal utility)
        public async Task<bool> ExistsAsync(Guid orderId)
        {
            return await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.Id == orderId);
        }

        // Get order with full details (OrderItems, Cancellations, etc.) for details page (Admin and Customer)
        public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.OrderCancellations)
                    .ThenInclude(c => c.CancellationItems)
                .Include(o => o.OrderReturns)
                    .ThenInclude(r => r.ReturnItems)
                .Include(o => o.Refunds)
                    .ThenInclude(rf => rf.RefundItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // Get paginated list of all orders (Admin, Reporting, most recent first)
        public async Task<List<Order>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Orders.AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get orders filtered by a specific status (Admin, Reporting)
        public async Task<List<Order>> GetByStatusAsync(OrderStatusEnum orderStatusId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Orders.AsNoTracking()
                .Where(o => o.OrderStatusId == orderStatusId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get orders within a date range (Reporting, Admin)
        public async Task<List<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
