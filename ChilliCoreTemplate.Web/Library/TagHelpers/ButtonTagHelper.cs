using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("button", Attributes = ActionAttribute)]
    public class ButtonTagHelper : TagHelper
    {
        private const string ActionAttribute = "mvc-action";
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ButtonTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        public ButtonStyle Style { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            output.Attributes.SetAttribute("type", "button");
            output.Attributes.AppendAttribute("class", $"btn btn-{Style.GetDescription().ToLower()} btn-sm");

            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var route = Action.GetRouteValueDictionary();
            route = route.AddRouteValues(RouteValues);

            var url = urlHelper.RouteUrl(route);
            output.Attributes.SetAttribute("onclick", $"window.location='{url}';");
        }
    }

    [HtmlTargetElement("button", Attributes = ActionAttribute)]
    public class ButtonTagModalHelper : TagHelper
    {
        private const string ActionAttribute = "mvc-modal";

        private readonly IUrlHelperFactory _urlHelperFactory;
        public ButtonTagModalHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        public ButtonStyle Style { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues });
            output.Attributes.SetAttribute("onclick", new HtmlString(url));

            output.Attributes.SetAttribute("type", "button");
            output.Attributes.AppendAttribute("class", $"btn btn-{Style.GetDescription().ToLower()} btn-sm");
        }
    }

    [HtmlTargetElement("buttonpost", Attributes = ActionAttribute)]
    public class ButtonPostTagHelper : TagHelper
    {
        private const string ActionAttribute = "mvc-action";
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ButtonPostTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        public ButtonStyle Style { get; set; }

        public string JsonData { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            output.Attributes.SetAttribute("type", "button");
            output.Attributes.AppendAttribute("class", $"btn btn-{Style.GetDescription().ToLower()} btn-sm");

            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var route = Action.GetRouteValueDictionary();
            route = route.AddRouteValues(RouteValues);

            var url = urlHelper.RouteUrl(route);
            output.Attributes.SetAttribute("onclick", $"$.doPost('{url}', {JsonData ?? "null"});");
        }
    }
}
