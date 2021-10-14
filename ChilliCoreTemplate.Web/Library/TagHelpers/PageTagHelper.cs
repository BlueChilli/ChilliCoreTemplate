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
        //<div id="page-container">
        //    <div class="row m-b-sm">
        //        <div class="col-lg-12">
        //        </div>
        //    </div>
        //</div>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("id", "page-container");
            output.PreContent.SetHtmlContent($"<div class=\"row m-b-sm\"><div class=\"col-lg-12\">");
            output.PostContent.SetHtmlContent("</div></div>");
        }
    }
}
