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
            output.Attributes.SetAttribute("class", $"card mb-10");
        }
    }

    public class BoxTitleTagHelper : TagHelper
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            output.Attributes.AppendAttribute("class", "card-header border-bottom d-flex align-items-center");

            var help = String.IsNullOrEmpty(Description) ? "" : $"<p class=\"text-sm text-muted\">{Description}</p>";
            if (String.IsNullOrEmpty(Title))
            {
                output.PreContent.SetHtmlContent("<h5 class=\"me-auto\">");
                output.PostContent.SetHtmlContent($" {help}</h5>");
            }
            else
            {
                output.PreContent.SetHtmlContent($"<div class=\"me-auto\"><h5>{Title}</h5>{help}</div>");
            }
        }
    }

    public class BoxContentTagHelper : TagHelper
    {
        public bool FullWidth { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            if (!FullWidth) output.AddClass("card-body", NullHtmlEncoder.Create());
        }
    }

    public class BoxFooterTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"card-footer border-0 py-5");
        }
    }
}
