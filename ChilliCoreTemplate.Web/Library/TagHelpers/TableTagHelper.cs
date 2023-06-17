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
}
