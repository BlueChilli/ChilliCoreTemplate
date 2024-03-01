using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class AlertTagHelper : TagHelper
    {
        public LabelType Type { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.AppendAttribute("class", $"alert alert-{Type.GetDescription()}");
        }
    }

    public class AlertTitleTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "h4";

            output.Attributes.AppendAttribute("class", "alert-heading");
        }
    }

}
