namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            //To Avoid runtime exception, we are initializing the Cliams property
            Cliams = new List<UserClaim>();
        }
        public string UserId { get; set; }
        public List<UserClaim> Cliams { get; set; }
    }
}
