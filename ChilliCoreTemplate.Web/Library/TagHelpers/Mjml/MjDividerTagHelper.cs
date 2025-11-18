using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ChilliCoreTemplate.Web.TagHelpers;

[HtmlTargetElement("mj-divider")]
public class MjDividerTagHelper : TagHelper
{
    [HtmlAttributeName("border-width")]
    public string BorderWidth { get; set; }

    [HtmlAttributeName("border-color")]
    public string BorderColor { get; set; }

    [HtmlAttributeName("border-style")]
    public string BorderStyle { get; set; } = "solid";

    [HtmlAttributeName("padding")]
    public string Padding { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "mj-divider";
        output.TagMode = TagMode.SelfClosing;

        output.Attributes.SetAttribute("border-width", BorderWidth ?? "1px");
        output.Attributes.SetAttribute("border-color", BorderColor ?? "#D9D9D9");
        output.Attributes.SetAttribute("border-style", BorderStyle ?? "solid");
        output.Attributes.SetAttribute("padding", Padding ?? "24px 12px");
    }
}