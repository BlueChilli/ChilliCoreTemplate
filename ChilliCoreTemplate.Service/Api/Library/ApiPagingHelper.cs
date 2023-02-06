using AutoMapper;

using ChilliCoreTemplate.Models.Api;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api
{
    public class PagingHelper<T> : ApiPagedList<T> where T : class
    {
        public static ApiPagedList<T> Create(IQueryable<T> query, Expression<Func<T, int>> idSelector, ApiPaging paging)
        {
            return Create(query, idSelector, (q) => q, paging);
        }

        public static ApiPagedList<T> Create<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, int>> idSelector, ApiPaging paging)
            where TEntity : class
        {
            return Create<TEntity>(query, idSelector, CreateMapper<TEntity>(), paging);
        }

        private static Func<IQueryable<TEntity>, IEnumerable<T>> CreateMapper<TEntity>()
            where TEntity : class
        {
            if (typeof(T).IsAssignableFrom(typeof(TEntity)))
                return (IQueryable<TEntity> q) => q.Cast<T>();

            return defaultMapper<TEntity>;
        }

        private static IEnumerable<T> defaultMapper<TEntity>(IQueryable<TEntity> query)
            where TEntity : class
        {
            foreach (var item in query)
            {
                yield return Mapper.Map<TEntity, T>(item);
            }
        }

        public static ApiPagedList<T> Create<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, int>> idSelector, Func<IQueryable<TEntity>, IEnumerable<T>> mapper, ApiPaging paging)
            where TEntity : class
        {
            //var maxId = paging.PagingMaxId;
            //if (maxId == null)
            //{
            //    maxId = query.Select(idSelector.Expand()).Select(r => (int?)r).Max() ?? 0;
            //}

            //Expression<Func<TEntity, bool>> filterMaxId = e => idSelector.Invoke(e) <= maxId;
            //query = query.Where(filterMaxId.Expand());

            var queryCount = query.Count();

            int skip = paging.PageSize.Value * (paging.PageNumber.Value - 1);
            var data = query.Skip(skip).Take(paging.PageSize.Value);

            var pageElements = mapper(data).ToList();
            var totalPages = (int)Math.Ceiling(queryCount / (double)paging.PageSize.Value);

            return new ApiPagedList<T>()
            {
                CurrentPage = paging.PageNumber.Value,
                Data = pageElements,
                PageCount = totalPages,
                PageSize = paging.PageSize.Value,
                TotalCount = queryCount
                //PagingMaxId = maxId
            };
        }

        public static ApiPagedList<T> CreateFrom<T2>(ChilliSource.Cloud.Core.PagedList<T2> from)
        {
            var data = Mapper.Map<List<T>>(from);
            var result = new ApiPagedList<T> { Data = data, CurrentPage = from.CurrentPage, PageCount = from.PageCount, PageSize = from.PageSize, TotalCount = from.TotalCount };
            return result;
        }

    }
}
