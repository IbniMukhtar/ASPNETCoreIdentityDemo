using System.ComponentModel.DataAnnotations;

namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class ConfirmPhoneNumberViewModel
    {
        [Display(Name = "Mobile Number:")]
        [Required(ErrorMessage = "Mobile Number is required.")]
/*        [RegularExpression("^([0-9]{10})$", ErrorMessage = "Invalid Mobile Number.")]*/
        public string? PhoneNumber { get; set; }
    }
}
