using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ChilliCoreTemplate.Models;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("icon")]
    public class IconTagHelper : TagHelper
    {
        public string Type { get; set; }

        public IconType? Icon { get; set; }

        public LabelType? Style { get; set; }

        public bool ShowTooltip { get; set; }

        public string Tooltip { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "i";
            output.AddClass("bi", NullHtmlEncoder.Default);

            var type = Icon.HasValue ? Icon.Value.GetData<string>("Icon") : Type;

            output.AddClass($"bi-{type}", NullHtmlEncoder.Default);
            if (Style.HasValue)
            {
                output.AddClass($"text-{Style.Value.GetDescription()}", NullHtmlEncoder.Default);
            }

            if (ShowTooltip)
            {
                output.Attributes.SetAttribute("data-bs-toggle", "tooltip");
                output.Attributes.SetAttribute("data-bs-original-title", Tooltip ?? Icon.GetDescription());
            }
        }
    }
}
