namespace OrderService.Domain.Entities
{
    public enum RefundStatusEnum
    {
        Pending = 1,            // Refund requested but not yet processed
        Processing = 2,         // Refund is being processed
        Completed = 3,          // Refund completed successfully
        Failed = 4,             // Refund failed (e.g., payment gateway error)
        PartiallyRefunded = 5,  // Partial refund processed
        Cancelled = 6           // Refund request cancelled
    }
}
