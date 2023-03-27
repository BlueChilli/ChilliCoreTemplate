using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("button", Attributes = ActionAttribute)]
    public class ButtonTagHelper : ButtonBaseTagHelper
    {
        private const string ActionAttribute = "mvc-action";

        public ButtonTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            base.Process(context, output);
            output.Attributes.RemoveAll(ActionAttribute);

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var route = Action.GetRouteValueDictionary();
            route = route.AddRouteValues(RouteValues);

            var url = urlHelper.RouteUrl(route);
            output.Attributes.SetAttribute("onclick", $"window.location='{url}';");
        }
    }

    [HtmlTargetElement("button", Attributes = ActionAttribute)]
    public class ButtonTagModalHelper : ButtonBaseTagHelper
    {
        private const string ActionAttribute = "mvc-modal";

        public ButtonTagModalHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            base.Process(context, output);
            output.Attributes.RemoveAll(ActionAttribute);

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues });
            output.Attributes.SetAttribute("onclick", new HtmlString(url));
        }
    }

    [HtmlTargetElement("buttonpost", Attributes = ActionAttribute)]
    public class ButtonPostTagHelper : ButtonBaseTagHelper
    {
        private const string ActionAttribute = "mvc-action";

        public ButtonPostTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        public string JsonData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            output.TagName = "button";

            base.Process(context, output);
            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var route = Action.GetRouteValueDictionary();
            route = route.AddRouteValues(RouteValues);

            var url = urlHelper.RouteUrl(route);
            var target = "";
            if (output.Attributes.ContainsName("target"))
            {
                target = output.Attributes["target"].Value.ToString();
                output.Attributes.RemoveAll("target");
            }
            output.Attributes.SetAttribute("onclick", $"$.doPost('{url}', '{target}', {JsonData ?? "null"});");
        }
    }

    public abstract class ButtonBaseTagHelper : TagHelper
    {
        protected readonly IUrlHelperFactory _urlHelperFactory;

        public ButtonBaseTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public ButtonStyle Style { get; set; }

        public IconType? Icon { get; set; }

        public string ToolTip { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("type", "button");
            output.Attributes.AppendAttribute("class", $"btn btn-{Style.GetDescription().ToLower()} btn-sm {(Icon.HasValue ? "btn-square" : "")}");

            if (Icon.HasValue)
            {
                output.Attributes.SetAttribute("data-bs-toggle", "tooltip");
                output.Attributes.SetAttribute("data-bs-original-title", Icon.GetDescription());
                output.PreContent.SetHtmlContent($"<i class=\"bi bi-{Icon.Value.GetData<string>("Icon")}\"></i>");
            }
        }
    }
}
