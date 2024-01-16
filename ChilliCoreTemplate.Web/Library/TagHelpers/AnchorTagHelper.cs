using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes = ActionAttribute)]
    public class AnchorTagHelper : TagHelper
    {
        protected const string ActionAttribute = "mvc-action";

        private readonly IUrlHelperFactory _urlHelperFactory;
        public AnchorTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var route = Action.GetRouteValueDictionary();
            route = route.AddRouteValues(RouteValues);

            var url = urlHelper.RouteUrl(route);
            output.Attributes.SetAttribute("href", url);
        }
    }

    [HtmlTargetElement("a", Attributes = ActionAttribute)]
    public class AnchorTagModalHelper : TagHelper
    {
        protected const string ActionAttribute = "mvc-modal";

        private readonly IUrlHelperFactory _urlHelperFactory;
        public AnchorTagModalHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues });
            output.Attributes.SetAttribute("onclick", url);
            output.Attributes.SetAttribute("href", "javascript: void(0);");
        }
    }

}
