namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class UserListViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public PaginationViewModel Pagination { get; set; }
    }
}
