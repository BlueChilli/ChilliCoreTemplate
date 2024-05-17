using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static partial class FieldTemplateHelper
    {
        //Drop down, when other is selected show other text box
        //@await Html.FieldOtherInnerForAsync(m => m.MyEnum, m => m.TextBoxOther, MyEnum.Other)

        public static async Task<IHtmlContent> FieldOtherInnerForAsync<TModel, TValue>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            Expression<Func<TModel, string>> otherExpression,
            TValue other
            )
        {
            var id = html.IdFor(expression);
            var content = new HtmlContentBuilder();

            content.AppendHtml("<div class=\"d-flex gap-2\">");

            content.AppendHtml(await html.FieldTemplateInnerForAsync(expression));

            var isOther = EqualityComparer<TValue>.Default.Equals(html.GetModelStateValue(expression), other);

            content.AppendHtml($"<div class=\"js-{id}-other {(isOther ? "" : "hide")}\">");

            content.AppendHtml(await html.FieldTemplateInnerForAsync(otherExpression));

            content.AppendHtml("</div></div>");

            html.RegisterCustomScript($"<script>$(function() {{ $('#{id}').change(function () {{ var other = $('.js-{id}-other'); if ($(this).val() == '{other}') {{ other.removeClass('hide'); }} else {{ other.addClass('hide');}} }});  }});</script>");
            return content;
        }

        public static void RegisterCustomScript(this IHtmlHelper html, string script)
        {
            Dictionary<string, Dictionary<Guid, IHtmlContent>> dictionary = html.ViewContext.HttpContext.Items["RenderCustomSection"] as Dictionary<string, Dictionary<Guid, IHtmlContent>>;
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, Dictionary<Guid, IHtmlContent>>();
                html.ViewContext.HttpContext.Items.Add("RenderCustomSection", dictionary);
            }

            Dictionary<Guid, IHtmlContent> dictionary2 = null;
            if (dictionary.ContainsKey("scripts"))
            {
                dictionary2 = dictionary["scripts"];
            }
            else
            {
                dictionary2 = new Dictionary<Guid, IHtmlContent>();
                dictionary.Add("scripts", dictionary2);
            }

            dictionary2.Add(Guid.NewGuid(), new HtmlString(script));
        }

    }
}