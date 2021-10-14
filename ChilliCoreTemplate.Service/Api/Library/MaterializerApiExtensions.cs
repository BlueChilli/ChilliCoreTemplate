using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqKit;
using ChilliSource.Cloud.Core.LinqMapper;


namespace ChilliCoreTemplate.Service.Api
{
    public static class MaterializerApiExtensions
    {
        public static ApiPagedList<TDest> ToPagedList<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer, ApiPaging apiPaging, bool previousPageIfEmpty = false)
        {
            var paged = materializer.To(q => q.ToPagedList(apiPaging.PageNumber.Value, apiPaging.PageSize.Value, previousPageIfEmpty));
            return ApiPagedList<TDest>.CreateFrom(paged);
        }

        //public static ApiPagedList<TDest> ToMaxIdPagedList<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer, Expression<Func<TDest, int>> idSelector, ApiPaging apiPaging, bool previousPageIfEmpty = false)
        //{
        //    int? maxId = apiPaging.PagingMaxId;
        //    materializer = materializer.Query(query =>
        //    {
        //        if (maxId == null)
        //        {
        //            maxId = query.Select(idSelector.Expand()).Select(r => (int?)r).Max() ?? 0;
        //        }

        //        Expression<Func<TDest, bool>> filterMaxId = e => idSelector.Invoke(e) <= maxId;
        //        return query.Where(filterMaxId.Expand());
        //    });

        //    var paged = materializer.ToPagedList(apiPaging, previousPageIfEmpty: previousPageIfEmpty);
        //    paged.PagingMaxId = maxId;

        //    return paged;
        //}
    }
}
