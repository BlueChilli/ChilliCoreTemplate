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
    public class HeadingTagHelper : TagHelper
    {
        public string Title { get; set; }

        //<strong>@Title</strong>
        //<p style="white-space: pre-wrap">@Content</p>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.PreContent.SetHtmlContent($"<strong>{Title}</strong><p style=\"white-space: pre-wrap\">");
            output.PostContent.SetHtmlContent("</p>");
        }
    }
}
