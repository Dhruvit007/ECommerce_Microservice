﻿using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class OrderStatus
    {
        [Key]
        public Guid Id { get; set; }

        //Pending, Confirmed, Packed, Shipped, Delivered, Cancelled, Returned
        [Required, MaxLength(50)]
        public OrderStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
