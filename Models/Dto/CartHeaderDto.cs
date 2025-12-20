using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SOLFranceBackend.Models.Dto
{
    public class CartHeaderDto
    {
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public double CartTotal { get; set; }
        public ICollection<CartDetailsDto>? CartDetailsList { get; set; }
    }
}
