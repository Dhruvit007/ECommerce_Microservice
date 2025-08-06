using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Order
{
    public class OrderResponseDTO
    {
        public Guid OrderId { get; set; }
        public string OrderName { get; set; } = null!;
        public Guid UserId { get; set; }
        public OrderStatusEnum OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemResponseDTO> Items { get; set; } = new();
        public Guid ShippingAddressId { get; set; }
        public AddressDTO ShippingAddress { get; set; } = null!;
        public Guid BillingAddressId { get; set; }
        public AddressDTO BillingAddress { get; set; } = null!;
        public PaymentMethodEnum PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal GrandTotal { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
