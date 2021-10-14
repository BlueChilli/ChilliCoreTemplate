using System;
using System.Collections.Generic;
using ChilliSource.Cloud.Core;

namespace ChilliCoreTemplate.Service
{
    public class ApiPagedList<T>
    {
        public ApiPagedList() { }

        public static ApiPagedList<T> CreateFrom(PagedList<T> list)
        {
            return new ApiPagedList<T>()
            {
                CurrentPage = list.CurrentPage,
                PageCount = list.PageCount,
                PageSize = list.PageSize,
                TotalCount = list.TotalCount,
                Data = list
            };
        }

        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int? PagingMaxId { get; set; }
        public List<T> Data { get; set; }
    }
}