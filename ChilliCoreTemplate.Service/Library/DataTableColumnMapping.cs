using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public class DataTableColumnMapping<T>
    {
        Dictionary<string, LambdaExpression> _columnMappings = new Dictionary<string, LambdaExpression>();

        public DataTableColumnMapping()
        {
            this.DefaultOrderAscending = true;
        }

        public void SetDefaultOrder<TKey>(Expression<Func<T, TKey>> defaultOrderExp, bool ascending)
        {
            this.DefaultOrderExpression = defaultOrderExp;
            this.DefaultOrderAscending = ascending;
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

        public bool DefaultOrderAscending { get; private set; }

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

        public void Add<TKey>(string columnId, Expression<Func<T, TKey>> mappingExpression)
        {
            _columnMappings[columnId.ToLower()] = mappingExpression;
        }

        public void Add(string columnId, LambdaExpression mappingExpression)
        {
            _columnMappings[columnId] = mappingExpression;
        }

        public LambdaExpression GetColumnMapOrDefault(string columnId)
        {
            if (_columnMappings.ContainsKey(columnId))
                return _columnMappings[columnId];

            var defaultExp = _columnMappings[columnId] = ToLambda(columnId);
            if (defaultExp == null)
                throw new ApplicationException(string.Format("No mapping found for column [{0}] and a default mapping expression could not be created.", columnId));

            return defaultExp;
        }

        private static IOrderedQueryable<T> OrderBy(IQueryable<T> query, LambdaExpression expression, bool ascending)
        {
            if (ascending)
                return Queryable.OrderBy(query, (dynamic)expression);
            else
                return Queryable.OrderByDescending(query, (dynamic)expression);
        }

        private static IOrderedQueryable<T> ThenBy(IOrderedQueryable<T> query, LambdaExpression expression, bool ascending)
        {
            if (ascending)
                return Queryable.ThenBy(query, (dynamic)expression);
            else
                return Queryable.ThenByDescending(query, (dynamic)expression);
        }

        private static string GetColumnId(IColumn column)
        {
            if (!String.IsNullOrEmpty(column.Name))
                return column.Name.ToLower();

            return column.Field.ToLower();
        }

        public IOrderedQueryable<T> ApplyOrder(IQueryable<T> query, IDataTablesRequest request)
        {
            var columnOrder = request.Columns.Where(c => c.IsSortable && c.Sort != null).OrderBy(c => c.Sort.Order).ToList();
            if (columnOrder.Count > 0)
            {
                var columnMap = this.GetColumnMapOrDefault(GetColumnId(columnOrder[0]));
                var sorted = OrderBy(query, columnMap, ascending: columnOrder[0].Sort.Direction == SortDirection.Ascending);
                foreach (var column in columnOrder.Skip(1))
                {
                    columnMap = this.GetColumnMapOrDefault(GetColumnId(column));
                    sorted = ThenBy(sorted, columnMap, ascending: columnOrder[0].Sort.Direction == SortDirection.Ascending);
                }

                return ThenBy(sorted, this.DefaultOrderExpression, ascending: this.DefaultOrderAscending);
            }
            else
            {
                return OrderBy(query, this.DefaultOrderExpression, ascending: this.DefaultOrderAscending);
            }
        }
    }

}
