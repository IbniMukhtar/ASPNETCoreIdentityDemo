using System.ComponentModel.DataAnnotations;

namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class ProfilePictureViewModel
    {
        [Required]
        public IFormFile? Picture { get; set; }
    }
}
