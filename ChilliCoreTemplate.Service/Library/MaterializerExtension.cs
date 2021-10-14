using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core.LinqMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public static class MaterializerExtension
    {
        public static ApiPagedList<TDest> ToPagedList<TSource, TDest>(this IQueryMaterializer<TSource, TDest> materializer, ApiPaging apiPaging, bool previousPageIfEmpty = false)
        {
            var result = materializer.ToPagedList(apiPaging.PageNumber.Value, apiPaging.PageSize.Value, previousPageIfEmpty);
            return ApiPagedList<TDest>.CreateFrom(result);
        }
    }
}
