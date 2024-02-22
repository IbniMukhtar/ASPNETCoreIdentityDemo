using System.ComponentModel.DataAnnotations;

namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class ChangePhoneNumberViewModel
    {
        [Required(ErrorMessage = "The Phone Number field is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [Display(Name = "New Phone Number")]
        public string? NewPhoneNumber { get; set; }
    }
}
