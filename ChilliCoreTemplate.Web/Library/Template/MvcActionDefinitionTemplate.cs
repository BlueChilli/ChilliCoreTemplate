using System;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ChilliCoreTemplate.Web
{
    public static partial class FieldTemplateHelper
    {
        public static Task<IHtmlContent> ButtonAsync(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, Template_Button options = null, MenuUrlValues urlValues = null)
        {
            return ButtonAsync(htmlHelper, actionResult, urlValues, options);
        }

        private static Template_Button PopulateOptions(IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues, Template_Button options)
        {
            if (options == null) options = new Template_Button();

            options.Url = options.Url ?? GetUrl(htmlHelper.GetUrlHelper(), actionResult, urlValues);
            options.Text = options.Text ?? (string)actionResult.GetRouteValueDictionary()["action"];

            return options;
        }

        public static Task<IHtmlContent> ButtonAsync(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues, Template_Button options = null)
        {
            options = PopulateOptions(htmlHelper, actionResult, urlValues, options);
            return ButtonAsync(htmlHelper, options);
        }

        public static Task<IHtmlContent> LinkAsync(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, long id, Template_Button options = null)
        {
            options = PopulateOptions(htmlHelper, actionResult, new MenuUrlValues(id), options);
            return LinkAsync(htmlHelper, options);
        }

        public static Task<IHtmlContent> LinkAsync(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, long id, string text, Template_Button options = null)
        {
            options = PopulateOptions(htmlHelper, actionResult, new MenuUrlValues(id), options);
            options.Text = text;
            return LinkAsync(htmlHelper, options);
        }

        public static Task<IHtmlContent> LinkAsync(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues = null, Template_Button options = null)
        {
            options = PopulateOptions(htmlHelper, actionResult, urlValues, options);
            return LinkAsync(htmlHelper, options);
        }

        public static IHtmlContent ModalOpen(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, long id)
        {
            return htmlHelper.ModalOpen(actionResult, new MenuUrlValues(id));
        }

        public static IHtmlContent ModalOpen(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues = null, string data = "null")
        {
            var command = htmlHelper.ModalOpenCommand(actionResult, urlValues, data);
            return MvcHtmlStringCompatibility.Create(command);
        }

        private static string GetUrl(IUrlHelper urlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues)
        {
            urlValues = urlValues ?? new MenuUrlValues();
            var routeValues = urlValues.RouteValues == null ? new RouteValueDictionary() : new RouteValueDictionary(urlValues?.RouteValues);
            if (!String.IsNullOrEmpty(urlValues.Id) && !routeValues.ContainsKey("id"))
            {
                routeValues["id"] = urlValues.Id;
            }

            if (routeValues != null && routeValues.Count > 0)
            {
                actionResult = actionResult.AddRouteValues(routeValues);
            }

            var url = urlHelper.RouteUrl(new UrlRouteContext()
            {
                RouteName = null,
                Values = actionResult.GetRouteValueDictionary(),
                Protocol = urlValues.Protocol ?? urlHelper.ActionContext.HttpContext.Request.Scheme,
                Fragment = urlValues.Fragment,
                Host = urlValues.HostName
            });

            if (string.IsNullOrEmpty(url))
                throw new ApplicationException("No route was found");

            return url;
        }

        public static string ModalOpenCommand(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues = null, string data = "null")
        {
            return ModelOpenCommand(htmlHelper.GetUrlHelper(), actionResult, urlValues, data);
        }

        public static string ModelOpenCommand(this IUrlHelper urlHelper, IMvcActionDefinition actionResult, MenuUrlValues urlValues = null, string data = "null")
        {
            var url = GetUrl(urlHelper, actionResult, urlValues);
            var id = actionResult.GetModalId();
            return $"$('#{id}_content').ajaxLoad({{url: '{url}', data: {data}}}).done(function() {{ $('#{id}').modal('show'); }});";
        }

        public static Task<IHtmlContent> ModalOpenJSAsync(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, Template_Button options = null, MenuUrlValues urlValues = null, string data = "null")
        {
            if (options == null) options = new Template_Button();
            options.Text = options.Text ?? actionResult.GetRouteValueDictionary()["action"] as string;

            options.Url = htmlHelper.ModalOpenCommand(actionResult, urlValues, data);

            return htmlHelper.LinkAsync(options);
        }
    }
}