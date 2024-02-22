using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ASPNETCoreIdentityDemo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "This FirstName field is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This  field LastName is required.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "This  field is required.")]
        public DateTime DOB { get; set; }
        public IEnumerable<ApplicationUser>? AllUsers { get; set; }
        public byte[]? ProfilePicture { get; set; }

    }
}