using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static partial class FieldTemplateHelper
    {
        public static Task<IHtmlContent> FieldDateRangeForAsync<TModel, DateTime>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, DateTime>> expression1,
            Expression<Func<TModel, DateTime>> expression2,
            Func<DateTime, string> valueFormat = null
            )
        {
            var validator = html.ViewContext.HttpContext.RequestServices.GetService<ValidationHtmlAttributeProvider>();

            var expressionProvider = new ModelExpressionProvider(html.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(html.ViewData, expression1).ModelExplorer;
            var model = (DateTime)explorer.Model;

            var name = html.NameFor(expression1);
            var htmlAttributes = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(html.ViewContext, explorer, name, htmlAttributes);

            var data = new FieldInnerTemplateModel
            {
                Id = html.IdFor(expression1).ToString(),
                Name = name,
                HtmlAttributes = (RouteValueDictionary)new RouteValueDictionary().Merge(htmlAttributes),
                Value = model
            };

            var explorer2 = expressionProvider.CreateModelExpression(html.ViewData, expression2).ModelExplorer;
            var model2 = (DateTime)explorer2.Model;

            var name2 = html.NameFor(expression2);
            var htmlAttributes2 = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(html.ViewContext, explorer, name2, htmlAttributes2);

            var data2 = new FieldInnerTemplateModel
            {
                Id = html.IdFor(expression2).ToString(),
                Name = name2,
                HtmlAttributes = (RouteValueDictionary)new RouteValueDictionary().Merge(htmlAttributes2),
                Value = model2
            };

            var viewData = new ViewDataDictionary(html.ViewData);
            viewData.Clear();
            viewData["ValueFormat"] = valueFormat;
            viewData["Model2"] = data2;

            return html.PartialAsync("FieldTemplates/DateRange", data, viewData);
        }
    }
}