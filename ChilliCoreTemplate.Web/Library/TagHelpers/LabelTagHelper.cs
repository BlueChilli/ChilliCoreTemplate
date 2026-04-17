using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ChilliCoreTemplate.Web.TagHelpers;

public class LabelTagHelper : TagHelper
{
    public LabelType Type { get; set; }

    //<span class="label label-@Model.Item3.GetDescription()">@Model.Item2</span>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Type == 0) return;
        output.TagName = "span";
        output.AddClass("badge", NullHtmlEncoder.Create());
        output.AddClass($"bg-{Type.GetDescription()}", NullHtmlEncoder.Create());
    }
}

public class LabelReverseTagHelper : TagHelper
{
    public LabelType Type { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Type == 0) return;
        output.TagName = "span";
        output.AddClass("badge", NullHtmlEncoder.Create());
        output.AddClass($"bg-soft-{Type.GetDescription()}", NullHtmlEncoder.Create());
        output.AddClass($"text-dark", NullHtmlEncoder.Create());
    }
}
