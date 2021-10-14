using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class ApiPaging
    {
        public static int DefaultPageSize = 40;

        public ApiPaging()
        {
            this.PageNumber = 1;
            this.PageSize = DefaultPageSize;
        }

        int _pageNumber;
        /// <summary>
        /// The page number of the results.  Note the first page is 1 (not 0). Defaults to first page.
        /// </summary>        
        public int? PageNumber
        {
            get { return _pageNumber; }
            set
            {
                // 1 <= x ; default 1
                _pageNumber = Math.Max(value ?? 1, 1);
            }
        }

        int _pageSize;
        /// <summary>
        /// The amount of items to display in each page.
        /// </summary>
        public int? PageSize
        {
            get { return _pageSize; }
            set
            {
                // 1 <= x <= 100 ; default 30
                _pageSize = Math.Min(100, Math.Max(value ?? DefaultPageSize, 1));
            }
        }

        //public int? PagingMaxId { get; set; }
    }
}
