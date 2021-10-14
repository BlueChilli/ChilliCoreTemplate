using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace ChilliCoreTemplate.Web
{
    public static class MvcActionDefinitionHtmlHelper
    {
        public static MvcForm BeginFormCustom(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, object htmlAttributes = null)
        {
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);

            if (!attributes.ContainsKey("id")) attributes["id"] = actionResult.GetFormId();

            // Set this attribute to String.Empty if you don't want form double submit prevention
            // This value is checked in jquery.bluechilli-mvc.js
            if (!attributes.ContainsKey("data-submitted")) attributes["data-submitted"] = "false";

            var method = FormMethod.Post;
            if (attributes.ContainsKey("method")) method = EnumHelper.Parse<FormMethod>(attributes["method"].ToString());

            var routeDict = actionResult.GetRouteValueDictionary();
            var actionName = routeDict["action"] as string;
            var controllerName = routeDict["controller"] as string;

            return htmlHelper.BeginForm(actionName, controllerName, routeDict, method, null, attributes);
        }

        public static string GetFormId(this IMvcActionDefinition actionResult)
        {
            var route = actionResult.GetRouteValueDictionary();
            var items = new string[] { "Form", (string)route["area"], (string)route["controller"], (string)route["action"] };

            return String.Join("_", items.Where(s => !String.IsNullOrWhiteSpace(s)));
        }

        public static string GetModalId(this IMvcActionDefinition actionResult)
        {
            var route = actionResult.GetRouteValueDictionary();
            var items = new string[] { "Modal", (string)route["area"], (string)route["controller"], (string)route["action"] };

            return String.Join("_", items.Where(s => !String.IsNullOrWhiteSpace(s)));
        }

        public static IHtmlContent MenuItem(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, string title = "", object routeValues = null, string linkClasses = null)
        {
            TagBuilder liTag = new TagBuilder("li");

            var url = htmlHelper.GetUrlHelper();
            actionResult = actionResult.AddRouteValues(routeValues);
            var routeDict = actionResult.GetRouteValueDictionary();

            var actionName = routeDict["action"] as string;
            var controllerName = routeDict["controller"] as string;
            var areaName = routeDict["area"] as string;

            var linkTag = ChilliSource.Cloud.Web.MVC.HtmlHelperExtensions.Link(url, actionName: actionName, controllerName: controllerName, area: areaName, routeValues: routeDict, displayText: title, linkClasses: linkClasses);
            liTag.SetInnerHtml(linkTag);

            //todo: set active somehow
            //if ((statusProvider == null && IsActive(routeValues))
            //  || (statusProvider != null && statusProvider.IsActive(this, routeValues)))
            //{
            //    liTag.AddCssClass("active");
            //}

            return liTag.AsHtmlContent();
        }

        public static IHtmlContent LinkPost(this IHtmlHelper htmlHelper, IMvcActionDefinition actionResult, string title = null, object routeValues = null, string linkClasses = null, string iconClasses = null, object linkAttributes = null, string confirmFunction = null)
        {
            var routeDict = actionResult.GetRouteValueDictionary();

            var actionName = routeDict["action"] as string;
            var controllerName = routeDict["controller"] as string;
            var areaName = routeDict["area"] as string;

            var link = ChilliSource.Cloud.Web.MVC.HtmlHelperExtensions.LinkPost(htmlHelper.GetUrlHelper(), actionName, controllerName, areaName, routeValues: routeValues, displayText: title, linkClasses: linkClasses, iconClasses: iconClasses, linkAttributes: linkAttributes, confirmFunction: confirmFunction);
            return link;
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, string linkText, IMvcActionDefinition result, object htmlAttributes = null, string protocol = null, string hostName = null, string fragment = null)
        {
            var routeValues = result.GetRouteValueDictionary();
            var action = (string)routeValues["action"];
            var controller = (string)routeValues["controller"];

            return htmlHelper.ActionLink(linkText, action, controller, protocol, hostName, fragment, routeValues, htmlAttributes);
        }
    }
}
