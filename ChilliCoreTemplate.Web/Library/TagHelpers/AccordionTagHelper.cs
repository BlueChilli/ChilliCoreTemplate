using ChilliCoreTemplate.Web.TagHelpers;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace acPortal.Web.TagHelpers
{

    public class AccordionTagHelper : TagHelper
    {
        public override void Init(TagHelperContext context)
        {
            var accordionContext = new AccordionContext
            {
                Id = $"accordion-{ShortGuid.NewGuid()}",
                Index = 0
            };
            context.Items.Add(typeof(AccordionTagHelper), accordionContext);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var accordionContext = AccordionContext.GetContext(context);

            output.TagName = null;
            output.PreContent.SetHtmlContent($"<div class=\"accordion\" id=\"{accordionContext.Id}\">");
            output.PostContent.SetHtmlContent($"</div>");
        }
    }

    public class AccordionItemTagHelper : TagHelper
    {
        public override void Init(TagHelperContext context)
        {
            var accordionContext = AccordionContext.GetContext(context);
            accordionContext.Index++;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            output.PreContent.SetHtmlContent($"<div class=\"accordion-item\">");
            output.PostContent.SetHtmlContent($"</div>");
        }
    }

    public class AccordionHeaderTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var accordionContext = AccordionContext.GetContext(context);
            var id = accordionContext.Id;
            var index = accordionContext.Index;

            output.TagName = "h2";
            output.Attributes.AppendAttribute("class", "accordion-header");

            output.PreContent.SetHtmlContent($"<button class=\"accordion-button {(index == 1 ? "" : "collapsed")}\" type=\"button\" data-bs-toggle=\"collapse\" data-bs-target=\"#{id}-{index}\">");
            output.PostContent.SetHtmlContent("</button>");
        }
    }

    public class AccordionBodyTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var accordionContext = AccordionContext.GetContext(context);
            var id = accordionContext.Id;
            var index = accordionContext.Index;

            output.TagName = "div";
            output.Attributes.SetAttribute("id", $"{id}-{index}");
            output.Attributes.AppendAttribute("class", $"accordion-collapse collapse {(index == 1 ? "show" : "")}");
            output.Attributes.SetAttribute("data-bs-parent", $"#{id}");

            output.PreContent.SetHtmlContent($"<div class=\"accordion-body\">");
            output.PostContent.SetHtmlContent("</div>");
        }
    }

    public class AccordionContext
    {
        public string Id { get; set; }

        public int Index { get; set; }

        public static AccordionContext GetContext(TagHelperContext context) => (AccordionContext)context.Items[typeof(AccordionTagHelper)];
    }
}