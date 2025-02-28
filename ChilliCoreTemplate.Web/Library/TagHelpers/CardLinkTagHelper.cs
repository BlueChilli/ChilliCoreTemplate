using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("card-link")]
    public class CardLinkTagHelper : CardLinkBaseTagHelper
    {
    }

    [HtmlTargetElement("card-link", Attributes = ActionAttribute)]
    public class CardLinkModalTagHelper : CardLinkBaseTagHelper
    {
        private const string ActionAttribute = "mvc-modal";
        private readonly IUrlHelperFactory _urlHelperFactory;

        public CardLinkModalTagHelper(IUrlHelperFactory urlHelperFactory) : base()
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [HtmlAttributeName(ActionAttribute)]
        public IMvcActionDefinition Action { get; set; }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Action == null)
                return;

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues }, data: "null");
            Link = url;
            IsModal = true;

            base.Process(context, output);
        }

    }

    public abstract class CardLinkBaseTagHelper : TagHelper
    {
        public string Link { get; set; }

        public ButtonWindow Window { get; set; }

        public string Image { get; set; }

        public IconType Icon { get; set; }

        public string IconClass { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        protected bool IsModal {get; set;  } 

        public CardLinkBaseTagHelper()
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "";

            var iconImage = Image != null ? $"<img src=\"{Image}\" class=\"avatar rounded\">" : $"<i class=\"bi bi-{Icon.GetData<string>("Icon")} {IconClass}\" style=\"font-size: 1.5em;\"></i>";

            var inner = $"<div class=\"card clickable\"><div class=\"p-5\"><div class=\"justify-content-between align-items-center\"><div class=\"d-flex align-items-center\"><div>{iconImage}</div><div class=\"ms-4\"><span class=\"h5 mb-1\">{Title}</span> <span class=\"d-block text-sm text-muted\">{Description}</span></div></div></div></div></div>";

            var html = IsModal ? $"<span onclick=\"{Link}\">{inner}</span>" : $"<a href=\"{Link}\" {(Window == ButtonWindow.New ? "target=\"_blank\"" : "")}>{inner}</a>";

            output.Content.SetHtmlContent(new HtmlString(html));
        }
    }



}
