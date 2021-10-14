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
    public class BoxTagHelper : TagHelper
    {
        //
        //<div class="ibox float-e-margins"> TODO what does float-e-margins do?
        //    <div class="ibox-title">
        //        <h5>title</h5>
        //    </div>
        //    <div class="ibox-content">
        //    </div>
        //</div>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("ibox", NullHtmlEncoder.Create());
        }
    }

    public class BoxTitleTagHelper : TagHelper
    {
        public string Title { get; set; }
        public string Help { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("ibox-title", NullHtmlEncoder.Create());

            var help = String.IsNullOrEmpty(Help) ? "" : $"<span class=\"ibox-title-help\">{Help}</span>";
            if(String.IsNullOrEmpty(Title))
            {
                output.PreContent.SetHtmlContent("<h5>");
                output.PostContent.SetHtmlContent($" {help}</h5>");
            }
            else
            {
                output.PreContent.SetHtmlContent($"<h5>{Title} {help}</h5>");
            }
        }
    }

    public class BoxContentTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("ibox-content", NullHtmlEncoder.Create());
        }
    }
}
