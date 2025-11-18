using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models.Api;

namespace ChilliCoreTemplate.Models;

/// <summary>
/// Extension methods for IEnumerable<T>
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Seeks a list for the requested page and returns a PagedList object.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    /// <param name="list">List of elements</param>
    /// <param name="page">Requested page</param>
    /// <param name="pageSize">Number of elements on each page.</param>        
    /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
    /// <returns>A PagedList object, containing the elements on the request page.</returns>
    public static PagedList<T> ToPagedList<T>(this IEnumerable<T> list, int page = 1, int pageSize = 10, bool previousPageIfEmpty = false)
    {
        var count = list.Count();
        var viewModel = new PagedList<T>
        {
            PageCount = (int)Math.Ceiling((float)count / pageSize),
            PageSize = pageSize,
            TotalCount = count,
            CurrentPage = page             
        };

        if (previousPageIfEmpty || page <= viewModel.PageCount)
        {
            viewModel.CurrentPage = Math.Max(1, Math.Min(page, viewModel.PageCount));
            viewModel.AddRange(list.Skip((viewModel.CurrentPage - 1) * pageSize).Take(pageSize));
        }

        return viewModel;
    }

    public static PagedList<T> ToPagedList<T>(this IEnumerable<T> list, ApiPaging paging)
    {
        return list.ToPagedList(paging.PageNumber ?? 1, paging.PageSize ?? 10);
    }

    public static void AddDistinct<T>(this List<T> list, T item)
    {
        if (!list.Contains(item)) list.Add(item);
    }
}
