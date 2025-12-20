using SOLFranceBackend.Models.Dto;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOLFranceBackend.Models
{
    public class CartDetails
    {
        [Key]
        public int CartDetailsId { get; set; }
        [ForeignKey("CartHeaderId")]
        public int CartHeaderId { get; set; }
        public int ProductId { get; set; }
        [NotMapped]
        public ProductDto? Product { get; set; }
        public int Count { get; set; }
        public CartHeader? CartHeader { get; set; }
    }
}
