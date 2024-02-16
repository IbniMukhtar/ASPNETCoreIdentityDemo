namespace ASPNETCoreIdentityDemo.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; private set;}
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public int TotalPages { get; private set; }
       
        public PaginationViewModel() { }
        public PaginationViewModel(int TotalItems,int Page,int PageSize) 
        {
            int totalItems = TotalItems;
            int totalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            int currentPage = Page;
            int pageSize = PageSize;
/*            int StartPage = CurrentPage - 5;
            int EndPAge = CurrentPage + 4;*/
            int startPage, endPage;
            if (totalPages <= 5)
            {
                startPage = 1;
                endPage = totalPages;
            }
            else
            {
                if (currentPage <= 6)
                {
                    startPage = 1;
                    endPage = 10;
                }
                else if (currentPage + 4 >= totalPages)
                {
                    startPage = totalPages - 9;
                    endPage = totalPages;
                }
                else
                {
                    startPage = currentPage - 5;
                    endPage = currentPage + 4;
                }
            }

            TotalItems = totalItems;
            TotalPages = totalPages;
            StartPage = startPage;  
            EndPage = endPage;
            PageSize = pageSize;
            CurrentPage = currentPage;
           

        }

        /*
        public int PrevPage => CurrentPage > 1 ? CurrentPage - 1 : 1;
        public int NextPage => CurrentPage < TotalPages ? CurrentPage + 1 : TotalPages;
        public bool IsFirstPageDisabled => CurrentPage == 1;
        public bool IsLastPageDisabled => CurrentPage == TotalPages;*/
    }

}
