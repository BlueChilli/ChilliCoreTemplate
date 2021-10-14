using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    [HtmlTargetElement("table-default")]
    public class TableTagHelper : TagHelper
    {
        public string Id { get; set; }
        public string Class { get; set; }

        //<div class="table-responsive">
        //    <table class="table table-striped table-bordered table-hover js-company-table">
        //    </table>
        //</div>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("table-responsive", NullHtmlEncoder.Create());
            output.PreContent.SetHtmlContent($"<table id=\"{Id}\" class=\"table table-striped table-bordered table-hover {Class}\">");
            output.PostContent.SetHtmlContent("</table>");
        }
    }

}
