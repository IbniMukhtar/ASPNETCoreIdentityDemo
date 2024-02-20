using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class RegisterViewModel
    {


        [Required(ErrorMessage = "This FirstName field is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This  LastName field is required.")]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

       
        [Display(Name = "DOB")]
        [DataType(DataType.Date)]
  /*      [MinimumAge(18)]*/
        [Required(ErrorMessage = "This DOB field is required.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email Address.")]
        [Remote(action: "IsEmailAvailable", controller: "Account")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [NotMapped]
        public string? ConfirmPassword { get; set; }
        // AuthenticationScheme is in Microsoft.AspNetCore.Authentication namespace
        public IList<AuthenticationScheme>? ExternalLogins { get; set; }
        public string? ReturnUrl { get; set; }
    }
}