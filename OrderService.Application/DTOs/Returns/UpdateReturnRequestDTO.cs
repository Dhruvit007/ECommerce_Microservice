using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Returns
{
    public class UpdateReturnRequestDTO
    {
        [Required]
        public Guid Id { get; set; } // Return Id to update
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public int ReasonId { get; set; }
        public string? Remarks { get; set; }
        public bool IsPartial { get; set; }
        public List<UpdateReturnItemDTO>? Items { get; set; }
    }
}
