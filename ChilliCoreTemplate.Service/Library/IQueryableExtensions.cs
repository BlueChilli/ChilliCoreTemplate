using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> HasExternalId<T>(this IQueryable<T> item, string externalId) where T : class, IExternalId
        {
            var hash = CommonLibrary.CalculateHash(externalId);
            return item.Where(x => x.ExternalIdHash == hash && x.ExternalId == externalId);
        }

    }
}
