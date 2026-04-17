using ChilliSource.Core.Extensions;
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

namespace ChilliCoreTemplate.Web.TagHelpers;

[HtmlTargetElement("card")]
public class CardTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.AppendAttribute("class", "card mb-4");

        output.PreContent.SetHtmlContent($"<div class=\"card-body\">");
        output.PostContent.SetHtmlContent($"</div>");
    }
}
