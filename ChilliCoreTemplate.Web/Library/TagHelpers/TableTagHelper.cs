using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("table-default")]
    public class TableTagHelper : TagHelper
    {
        public string Id { get; set; }
        public string Class { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";
            output.Attributes.SetAttribute("class", $"table table-hover {Class}");
            output.Attributes.SetAttribute("width", "100%");
        }
    }

    [HtmlTargetElement("tablebutton", Attributes = ActionAttribute)]
    public class TableButtonTagHelper : ButtonBaseTagHelper
    {
        private const string ActionAttribute = "mvc-action";

        public TableButtonTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
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

            output.TagName = "button";

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            RouteValues.Add("id", "$$$");
            var route = Action.GetRouteValueDictionary();
            route = route.AddRouteValues(RouteValues);

            var url = urlHelper.RouteUrl(route);
            output.Attributes.SetAttribute("onclick", new HtmlString($"window.location='{url}';".Replace("'", @"\'").Replace("$$$", "{0}")));
        }
    }

    //return '@{<tablebutton mvc-modal="Mvc.Admin.Widget_Detail" icon="View"></tablebutton>}'.format(row.id);
    [HtmlTargetElement("tablebutton", Attributes = ActionAttribute)]
    public class TableButtonTagModalHelper : ButtonBaseTagHelper
    {
        private const string ActionAttribute = "mvc-modal";

        public TableButtonTagModalHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
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

            output.TagName = "button";

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues }, data: "{id: $(this).data('id')} "); //Space is needed
            output.Attributes.SetAttribute("onclick", new HtmlString(url.Replace("'", @"\'")));
            output.Attributes.SetAttribute("data-id", "{0}");
        }
    }
}
