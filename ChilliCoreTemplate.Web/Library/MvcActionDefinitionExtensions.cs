using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class MvcActionDefinitionExtensions
    {
        public static ActionResult Redirect(this IMvcActionDefinition actionResult, Controller controller, object routeValues = null, string protocol = null, string fragment = null)
        {
            var url = actionResult.Url(controller.Url, routeValues: routeValues, protocol: protocol, fragment);

            if (controller.Request.IsAjaxRequest())
            {
                controller.Response.Headers["X-Ajax-Redirect"] = url;
                return new EmptyResult();
            }
            else
            {
                return controller.Redirect(url);
            }
        }

        public static ActionResult Redirect(this IMvcActionDefinition actionResult, Controller controller, int id)
        {
            return actionResult.Redirect(controller, routeValues: new { Id = id });
        }

        public static ActionResult RedirectWithError(this IMvcActionDefinition actionResult, Controller controller, string error)
        {
            controller.TempData[PageMessage.Key()] = PageMessage.Error(error);
            return Redirect(actionResult, controller);
        }

        public static string Url(this IMvcActionDefinition actionResult, Controller controller, object routeValues = null, string protocol = null, string fragment = null, bool throwsNotFound = true)
        {
            return actionResult.Url(controller.Url, routeValues: routeValues, protocol: protocol, fragment: fragment, throwsNotFound: throwsNotFound);
        }

        public static string Url(this IMvcActionDefinition actionResult, IUrlHelper urlHelper, int id)
        {
            return actionResult.Url(urlHelper, routeValues: new { Id = id });
        }

        public static string Url(this IMvcActionDefinition actionResult, IUrlHelper urlHelper, int id, object routeValues)
        {
            actionResult = actionResult.AddRouteId(id);

            return actionResult.Url(urlHelper, routeValues);
        }

        public static string Url(this IMvcActionDefinition actionResult, IUrlHelper urlHelper, object routeValues = null, string protocol = null, string fragment = null, bool throwsNotFound = true)
        {
            if (routeValues != null)
            {
                actionResult = actionResult.AddRouteValues(routeValues);
            }

            var url = urlHelper.RouteUrl(new UrlRouteContext()
            {
                RouteName = null,
                Values = actionResult.GetRouteValueDictionary(),
                Protocol = protocol ?? urlHelper.ActionContext.HttpContext.Request.Scheme,
                Fragment = fragment
            });

            if (throwsNotFound && string.IsNullOrEmpty(url))
                throw new ApplicationException("No route was found");

            return url;
        }
        public static bool IsCurrent(this IMvcActionDefinition actionResult, IUrlHelper urlHelper)
        {
            var routeValues = actionResult.GetRouteValueDictionary();
            return urlHelper.IsCurrent(routeValues);
        }

        public async static Task<bool> IsRefererAsync(this IMvcActionDefinition actionResult, HttpContext context)
        {
            if (!Uri.TryCreate(context.Request.Headers.Referer.FirstOrDefault(), UriKind.Absolute, out var uri))
                return false;

            var refererContext = new DefaultHttpContext();
            refererContext.Request.Path = uri.PathAndQuery;

            var routeContext = new RouteContext(refererContext);
            var router = context.GetRouteData().Routers.First();

            await router.RouteAsync(routeContext); //router updates routeContext

            var routeValues = new RouteValueDictionary(actionResult.GetRouteValueDictionary());
            var refererValues = routeContext.RouteData.Values;

            if (RouteHelper.CurrentArea(routeValues).Same(RouteHelper.CurrentArea(refererValues)))
            {
                if (RouteHelper.CurrentController(routeValues).Same(RouteHelper.CurrentController(refererValues)))
                {
                    return RouteHelper.CurrentAction(routeValues).Same(RouteHelper.CurrentAction(refererValues));
                }
            }

            return false;
        }

        public static async Task<IHtmlContent> ModalOpenLinkAsync<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, string text, object routeValues, object htmlAttributes = null)
        {
            var modalCommand = htmlHelper.ModalOpen(actionResult, new MenuUrlValues { RouteValues = routeValues });
            if (htmlAttributes == null) htmlAttributes = new { };
            var dictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            dictionary.AddOrSkipIfExists("onclick", modalCommand);
            return await htmlHelper.LinkAsync(new Template_Button { Url = "#", Text = text, HtmlAttributes = dictionary });
        }

        public static IHtmlContent ModalOpen<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, int id)
        {
            return htmlHelper.ModalOpen(actionResult, new MenuUrlValues(id));
        }

        public static IHtmlContent ModalOpen<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, object routeValues = null, string data = "null")
        {
            return htmlHelper.ModalOpen(actionResult, new MenuUrlValues { RouteValues = routeValues }, data);
        }

        public static async Task<IHtmlContent> LinkAsync<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, int id, string text, object routeValues = null, object htmlAttributes = null)
        {
            return await htmlHelper.LinkAsync(actionResult, new MenuUrlValues(id) { RouteValues = routeValues }, new Template_Button { Text = text, HtmlAttributes = htmlAttributes });
        }

        public static async Task<IHtmlContent> LinkAsync<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, string text, object routeValues = null, object htmlAttributes = null)
        {
            return await htmlHelper.LinkAsync(actionResult, new MenuUrlValues() { RouteValues = routeValues }, new Template_Button { Text = text, HtmlAttributes = htmlAttributes });
        }

        public static IHtmlContent ButtonPost<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, int id, string text, object routeValues = null, object htmlAttributes = null, string buttonClasses = "")
        {
            var routeData = actionResult.GetRouteValueDictionary();

            return htmlHelper.ButtonPost(routeData["action"].ToString(), routeData["controller"].ToString(), routeData["area"].ToString(), id: id.ToString(), routeValues: routeValues, displayText: text, buttonClasses: buttonClasses, buttonAttributes: htmlAttributes);
        }

        public static IHtmlContent ButtonPost<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, string text, object routeValues = null, object htmlAttributes = null, string buttonClasses = "")
        {
            var routeData = actionResult.GetRouteValueDictionary();

            return htmlHelper.ButtonPost(routeData["action"].ToString(), routeData["controller"].ToString(), routeData["area"].ToString(), routeValues: routeValues, displayText: text, buttonClasses: buttonClasses, buttonAttributes: htmlAttributes);
        }

        public static async Task<IHtmlContent> ButtonSubmitAsync<T>(this IMvcActionDefinition actionResult, IHtmlHelper<T> htmlHelper, string text)
        {
            return await htmlHelper.ButtonSubmitAsync(text);
        }
    }
}
