using DataTables.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ChilliCoreTemplate.Models.Api
{
    public class ApiPaging : IApiOrderable
    {
        public static int DefaultPageSize = 40;
        public static int MaxPageSize = 100;
        public static readonly ApiPaging Max = new ApiPaging() { _pageSize = MaxPageSize };

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
                _pageSize = Math.Min(MaxPageSize, Math.Max(value ?? DefaultPageSize, 1));
            }
        }

        //public int? PagingMaxId { get; set; }

        public string SortField { get; set; }

        public SortDirection SortDirection { get; set; }

    }

    public interface IApiOrderable
    {
        public string SortField { get; set; }

        public SortDirection SortDirection { get; set; }
    }

    public class ApiColumnMapping<T>
    {
        Dictionary<string, LambdaExpression> _columnMappings = new Dictionary<string, LambdaExpression>();

        public ApiColumnMapping()
        {
            this.DefaultOrder = SortDirection.Ascending;
        }

        public void SetDefaultOrder<TKey>(Expression<Func<T, TKey>> defaultOrderExp, SortDirection direction)
        {
            this.DefaultOrderExpression = defaultOrderExp;
            this.DefaultOrder = direction;
        }

        LambdaExpression _DefaultOrderExpression;
        public LambdaExpression DefaultOrderExpression
        {
            get
            {
                if (_DefaultOrderExpression == null)
                {
                    _DefaultOrderExpression = ToLambda("Id");
                    if (_DefaultOrderExpression == null)
                        throw new ApplicationException(string.Format("No default order found for type [{0}] and a default order expression could not be created.", typeof(T).FullName));
                }

                return _DefaultOrderExpression;
            }
            private set { _DefaultOrderExpression = value; }
        }

        public SortDirection DefaultOrder { get; private set; }

        private static LambdaExpression ToLambda(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException(String.Format("{0} is empty.", propertyName));

            try
            {
                var parameterExp = Expression.Parameter(typeof(T), "p");
                var propertyExp = Expression.Property(parameterExp, propertyName);

                return Expression.Lambda(propertyExp, parameterExp);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Map source to underlying data sourc
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source">eg nameof(Class.Property)</param>
        /// <param name="mappingExpressionDest">eg x => x.Property</param>
        public void Add<TKey>(string source, Expression<Func<T, TKey>> mappingExpressionDest)
        {
            _columnMappings[source.ToLower()] = mappingExpressionDest;
        }

        //public void Add(string columnId, LambdaExpression mappingExpression)
        //{
        //    _columnMappings[columnId] = mappingExpression;
        //}

        public void AutoMap(params string[] sources)
        {
            foreach (var source in sources)
            {
                var defaultExp = ToLambda(source);
                if (defaultExp == null)
                    throw new ApplicationException($"A default mapping expression could not be created for [{source}].");
                _columnMappings[source.ToLower()] = defaultExp;
            }
        }

        public LambdaExpression GetColumnMap(string columnId)
        {
            columnId = columnId.ToLower();

            if (_columnMappings.ContainsKey(columnId))
                return _columnMappings[columnId];

            throw new ApplicationException($"No mapping found for column [{columnId}]");
        }

        private static IOrderedQueryable<T> OrderBy(IQueryable<T> query, LambdaExpression expression, SortDirection direction)
        {
            if (direction == SortDirection.Ascending)
                return Queryable.OrderBy(query, (dynamic)expression);
            else
                return Queryable.OrderByDescending(query, (dynamic)expression);
        }

        public IOrderedQueryable<T> ApplyOrder(IQueryable<T> query, IApiOrderable model)
        {
            if (String.IsNullOrEmpty(model.SortField))
            {
                return OrderBy(query, this.DefaultOrderExpression, this.DefaultOrder);
            }

            var columnMap = this.GetColumnMap(model.SortField);
            return OrderBy(query, columnMap, model.SortDirection);
        }
    }
}
