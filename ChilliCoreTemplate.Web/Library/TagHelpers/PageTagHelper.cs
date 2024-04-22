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
    public class PageTagHelper : TagHelper
    {
        public ModalSize Size { get; set; } = ModalSize.ExtraLarge;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var sizeClass = Size == ModalSize.Medium ? "max-w-screen-md" : "";

            output.TagName = "main";
            output.Attributes.SetAttribute("id", "page-container");
            output.Attributes.SetAttribute("class", "py-6 bg-surface-secondary");
            output.PreContent.SetHtmlContent($"<div class=\"container-fluid {sizeClass}\"><div class=\"vstack gap-4\">");
            output.PostContent.SetHtmlContent("</div></div>");
        }
    }
}
