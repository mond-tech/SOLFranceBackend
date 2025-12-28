using System.ComponentModel.DataAnnotations;

namespace SOLFranceBackend.Models
{
    public class ContactUs
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(150)]
        public string JobTitle { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string WorkEmail { get; set; }

        [Phone]
        [MaxLength(30)]
        public string PhoneNumber { get; set; }

        [MaxLength(200)]
        public string CompanyName { get; set; }

        [Url]
        [MaxLength(300)]
        public string Website { get; set; }

        [Required]
        [MaxLength(2000)]
        public string ProjectRequirements { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
