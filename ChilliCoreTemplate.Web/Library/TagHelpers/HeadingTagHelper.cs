using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class HeadingsCardTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.AppendAttribute("class", "card");
        }
    }

    public class HeadingsTitleTagHelper : TagHelper
    {
        public string Title { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.AppendAttribute("class", "card-header d-flex align-items-center");

            output.PreContent.SetHtmlContent(Title);
        }
    }

    /// <summary>
    /// Will create card wrapper if title passed in (shortcut instead of using headings-card and headings-title)
    /// </summary>
    public class HeadingsTagHelper : TagHelper
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var @class = context.AllAttributes.Any(x => x.Name == "class") ? context.AllAttributes["class"].Value : "";

            var title = "";
            var hasTitle = !String.IsNullOrEmpty(Title);
            if (hasTitle)
            {
                var description = !String.IsNullOrEmpty(Description) ? $"<p class=\"text-sm text-muted\">{Description}</p>" : "";
                title = $"<div class=\"card\"><div class=\"card-header {@class}\">{Title}{description}</div>";
                @class = "";
            }
            output.PreContent.SetHtmlContent($"{title}<div class=\"list-group {@class}\">");
            output.PostContent.SetHtmlContent($"</div>{(hasTitle ? "</div>" : "")}");
        }
    }

    public class HeadingTagHelper : TagHelper
    {
        public string Title { get; set; }

        public bool PreWrap { get; set; }

        public HeadingFormat Format { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            if (Format == HeadingFormat.Inline)
            {
                output.PreContent.SetHtmlContent($"<div class=\"list-group-item d-flex justify-content-between align-items-start\"><div class=\"ms-2\"><div><strong>{Title}</strong></div></div>{(PreWrap ? "<span style=\"white-space: pre-wrap\">" : "")}");
                output.PostContent.SetHtmlContent($"{(PreWrap ? "</span>" : "")}</div>");
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
