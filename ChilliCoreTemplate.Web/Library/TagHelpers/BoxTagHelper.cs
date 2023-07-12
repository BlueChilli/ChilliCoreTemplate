using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class BoxTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.AppendAttribute("class", $"card mb-10");
        }
    }

    public class BoxTitleTagHelper : TagHelper
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            output.Attributes.AppendAttribute("class", "card-header border-bottom d-flex align-items-center");

            var help = String.IsNullOrEmpty(Description) ? "" : Description.Length < 50 ? $"<span class=\"text-sm text-muted\">{Description}</span>" : $"<p class=\"text-sm text-muted\">{Description}</p>";
            if (String.IsNullOrEmpty(Title))
            {
                output.PreContent.SetHtmlContent("<h5 class=\"me-auto\">");
                output.PostContent.SetHtmlContent($" {help}</h5>");
            }
            else
            {
                output.PreContent.SetHtmlContent($"<h5 class=\"me-auto\">{Title} {help}</h5>");
            }
        }
    }

    public class BoxContentTagHelper : TagHelper
    {
        public bool FullWidth { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            if (!FullWidth) output.AddClass("card-body", NullHtmlEncoder.Create());
        }
    }

    public class BoxFooterTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"card-footer border-0 py-5");
        }
    }
}
