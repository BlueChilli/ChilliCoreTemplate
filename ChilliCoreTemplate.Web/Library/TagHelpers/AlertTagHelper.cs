using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers;

public class AlertTagHelper : TagHelper
{
    public LabelType Type { get; set; }

    public bool IsDismissible { get; set; }

    public bool AutoDismiss { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        var dismissisble = IsDismissible ? $" alert-dismissible fade show {(AutoDismiss ? "js-page-message" : "")}" : "";
        output.Attributes.AppendAttribute("class", $"alert alert-{Type.GetDescription()}{dismissisble} ");
        if (IsDismissible) output.PreContent.AppendHtml("<button aria-hidden=\"true\" data-bs-dismiss=\"alert\" class=\"btn-close\" type=\"button\"></button>");

        if (AutoDismiss)
        {
            var script = "<script>window.setTimeout(function () { AutoDismissPageMessage(); }, 4000);</script>";
            CustomScriptsHelper.RegisterCustomScripts(ViewContext, new Guid("8256BBE9-6A9D-493F-A1F6-2EA662BB8A56"), script);
        }
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
