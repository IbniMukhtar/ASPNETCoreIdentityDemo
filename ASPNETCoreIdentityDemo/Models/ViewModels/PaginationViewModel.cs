namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public int TotalPages { get; private set; }

        public PaginationViewModel() { }
        public PaginationViewModel(int totalItems, int page, int pageSize)
        {
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            CurrentPage = page;
            PageSize = pageSize;

            if (TotalPages <= 10)
            {
                StartPage = 1;
                EndPage = TotalPages;
            }
            else
            {
                if (page <= 6)
                {
                    StartPage = 1;
                    EndPage = 10;
                }
                else if (page + 4 >= TotalPages)
                {
                    StartPage = TotalPages - 9;
                    EndPage = TotalPages;
                }
                else
                {
                    StartPage = page - 5;
                    EndPage = page + 4;
                }
            }
        }
    }
}
