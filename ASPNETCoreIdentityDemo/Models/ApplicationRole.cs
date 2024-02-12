using Microsoft.AspNetCore.Identity;

namespace ASPNETCoreIdentityDemo.Models
{
    public class ApplicationRole : IdentityRole
    {
        // Add custom properties here
        public string? Description { get; set; }
    }
}