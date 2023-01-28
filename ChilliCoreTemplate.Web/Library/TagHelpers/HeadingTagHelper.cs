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
    public class HeadingsTagHelper : TagHelper
    {
        public string Title { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var title = "";
            var hasTitle = !String.IsNullOrEmpty(Title);
            if (hasTitle)
            {
                var @class = context.AllAttributes.Any(x => x.Name == "class") ? context.AllAttributes["class"].Value : "";
                title = $"<div class=\"card\"><div class=\"card-header {@class}\">{Title}</div>";
            }
            output.PreContent.SetHtmlContent($"{title}<div class=\"list-group\">");
            output.PostContent.SetHtmlContent($"</div>{(hasTitle ? "</div>" : "")}");
        }
    }

    public class HeadingTagHelper : TagHelper
    {
        public string Title { get; set; }

        public bool PreWrap { get; set; } = true;

        //<p style=\"{(PreWrap ? "white-space: pre-wrap" : "")}\">

        public HeadingFormat Format { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            if (Format == HeadingFormat.Inline)
            {
                output.PreContent.SetHtmlContent($"<div class=\"list-group-item d-flex justify-content-between align-items-start\"><div class=\"ms-2\"><div><strong>{Title}</strong></div></div>");
                output.PostContent.SetHtmlContent("</div>");
            }
            else
            {
                output.PreContent.SetHtmlContent($"<div class=\"list-group-item\"><div class=\"ms-2\"><div><strong>{Title}</strong></div></div><div class=\"ms-2 mt-4\">");
                output.PostContent.SetHtmlContent("</div></div>");
            }
        }
    }

    public class HeadingItemsTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            output.PreContent.SetHtmlContent($"<div class=\"d-flex flex-column\">");
            output.PostContent.SetHtmlContent($"</div>");
        }
    }

    public class HeadingItemTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            output.PreContent.SetHtmlContent($"<div class=\"mb-1\">");
            output.PostContent.SetHtmlContent($"</div>");
        }
    }
}

namespace ChilliCoreTemplate.Web
{
    public enum HeadingFormat
    {
        Inline,
        Under
    }
}
