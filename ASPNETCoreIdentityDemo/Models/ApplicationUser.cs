using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ASPNETCoreIdentityDemo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "This field is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This Email field is required.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "This Email field is required.")]
        [DataType(DataType.DateTime)]
        public DateTime DOB { get; set; }
    }
}