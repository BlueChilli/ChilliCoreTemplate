using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("required")]
    public class RequiredTagHelper : TagHelper
    {
        public List<string> Params { get; set; } = [];

        public string Param { get { return Params.FirstOrDefault(); } set { Params.Add(value); } }
 
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Params.Any(string.IsNullOrEmpty))
            {
                output.Content.SetHtmlContent("<span class=\"badge bg-danger\">Required</span>");
            }
            else
            {
                output.TagName = null;
            }
        }
    }
}
