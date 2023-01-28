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
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class TabsContainerTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("tabs-container", NullHtmlEncoder.Create());
        }
    }

    public class TabsTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "ul";
            output.AddClass("nav", HtmlEncoder.Default);
            output.AddClass("nav-tabs", HtmlEncoder.Default);
            output.AddClass("overflow-x", HtmlEncoder.Default);
            output.AddClass("border-0", HtmlEncoder.Default);
            output.Attributes.Add("role", "tablist");
        }
    }

    //<ul class="nav nav-tabs overflow-x border-0"><li class="nav-item"><a href = "#" class="nav-link active">View all</a></li><li class="nav-item"><a href = "#" class="nav-link">Most recent</a></li><li class="nav-item"><a href = "#" class="nav-link">Popular</a></li></ul>

    public class TabTagHelper : TagHelper
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "li";
            output.AddClass("nav-item", HtmlEncoder.Default);

            output.PreContent.SetHtmlContent($"<a class=\"nav-link {(IsActive ? "active" : "")}\" data-bs-toggle=\"tab\" data-bs-target=\"#tab-{Id}\" role=\"tab\">");
            output.PostContent.SetHtmlContent($"</a>");
        }
    }

    public class TabContentTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("tab-content", NullHtmlEncoder.Create());
        }
    }

    public class TabPaneTagHelper : TagHelper
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("tab-pane", HtmlEncoder.Default);
            output.AddClass("fade", HtmlEncoder.Default);
            if (IsActive)
            {
                output.AddClass("active", HtmlEncoder.Default);
                output.AddClass("show", HtmlEncoder.Default);
            }
            output.Attributes.Add("id", $"tab-{Id}");
            output.Attributes.Add("role", "tabpanel");

            output.PreContent.SetHtmlContent("<div class=\"panel-body\">");
            output.PostContent.SetHtmlContent("</div>");
        }
    }
}
