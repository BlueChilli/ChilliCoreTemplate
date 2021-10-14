using ChilliCoreTemplate.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    public class Select2QueryModel<TSource, TResult>
    {
        public Select2QueryModel(ApiPagedList<TSource> list, Func<TSource, TResult> selector)
        {
            results = list.Data.Select(x => selector(x)).ToList();
            pagination = new Select2QueryPagination { more = list.PageCount > list.CurrentPage };
        }

        public List<TResult> results { get; set; }

        public Select2QueryPagination pagination { get; set; }
    }

    public class Select2QueryPagination
    {
        public bool more { get; set; }
    }
}
