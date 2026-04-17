using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class HeadingsCardTagHelper : TagHelper
    {
        public HeadingSkin Skin { get; set; }

        public override void Init(TagHelperContext context)
        {
            context.Items.Add("Skin", Skin);
        }

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
        public HeadingSkin Skin { get; set; }

        public override void Init(TagHelperContext context)
        {
            if (context.Items.ContainsKey("Skin"))
                Skin = (HeadingSkin)context.Items["Skin"];
            else
                context.Items.Add("Skin", Skin);
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var title = "";
            var hasTitle = !String.IsNullOrEmpty(Title);
            if (hasTitle)
            {
                var @class = context.AllAttributes.Any(x => x.Name == "class") ? context.AllAttributes["class"].Value : "";

                title = Skin == HeadingSkin.Standard
                    ? $"<div class=\"card\"><div class=\"card-header {@class}\">{Title}</div>"
                    : $"<div class=\"card\"><div class=\"card-header border-bottom {@class}\"><h3 class=\"h3\">{Title}</h3></div>";
            }
            var wrapperClass = Skin == HeadingSkin.Standard ? "list-group" : "card-body";
            output.PreContent.SetHtmlContent($"{title}<div class=\"{wrapperClass}\">");
            output.PostContent.SetHtmlContent($"</div>{(hasTitle ? "</div>" : "")}");
        }
    }

    public class HeadingSubtitleTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var skin = (HeadingSkin)context.Items["Skin"];

            if (skin == HeadingSkin.Standard)
            {
                output.TagName = null;
            }
            else
            {
                output.TagName = "h4";
                output.Attributes.AppendAttribute("class", "h4 mb-2");
            }
        }
    }

    public class HeadingTagHelper : TagHelper
    {
        public string Title { get; set; }

        public bool PreWrap { get; set; }

        public HeadingFormat Format { get; set; }

        public string Tooltip { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var skin = context.Items.ContainsKey("Skin") ? (HeadingSkin)context.Items["Skin"] : HeadingSkin.Standard;
            output.TagName = null;

            var toolTipHtml = "";
            if (!String.IsNullOrEmpty(Tooltip))
            {
                toolTipHtml = $" <i class=\"bi bi-question-circle-fill\" data-bs-toggle=\"tooltip\" data-bs-original-title=\"{Tooltip}\" data-bs-html=\"true\"></i>";
            }

            if (Format == HeadingFormat.Inline)
            {
                var content = skin == HeadingSkin.Standard
                    ? $"<div class=\"list-group-item d-flex justify-content-between align-items-start\"><div class=\"ms-2\"><div><strong>{Title}</strong>{toolTipHtml}</div></div>{(PreWrap ? "<span style=\"white-space: pre-wrap\">" : "")}"
                    : $"<div class=\"row mb-2\"><div class=\"col-4 font-semibold\">{Title}</div><div class=\"col-8\">";

                output.PreContent.SetHtmlContent(content);
                output.PostContent.SetHtmlContent(skin == HeadingSkin.Standard ? $"{(PreWrap ? "</span>" : "")}</div>" : "</div></div>");
            }
            else
            {
                output.PreContent.SetHtmlContent($"<div class=\"list-group-item\"><div class=\"ms-2\"><div><strong>{Title}</strong>{toolTipHtml}</div></div><div class=\"ms-2 mt-4\">");
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
    public enum HeadingSkin
    {
        Standard,
        Compact
    }

    public enum HeadingFormat
    {
        Inline,
        Under
    }
}
