using Microsoft.AspNetCore.Identity;

namespace Marketplace.Models
{
    // Identity user with int key to align with domain IDs
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public string? ImagemPerfil { get; set; }
    }
}

