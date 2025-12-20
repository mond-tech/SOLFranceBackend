using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOLFranceBackend.Models
{
    public class CartHeader
    {
        [Key]
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public ICollection<CartDetails>? CartDetailsList { get; set; }
        [NotMapped]
        public double CartTotal { get; set; }
    }
}
