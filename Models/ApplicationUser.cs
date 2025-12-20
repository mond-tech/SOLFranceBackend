using Microsoft.AspNetCore.Identity;

namespace SOLFranceBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
