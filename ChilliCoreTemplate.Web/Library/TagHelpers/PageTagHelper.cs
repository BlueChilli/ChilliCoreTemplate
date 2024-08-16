using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ChilliCoreTemplate.Web.TagHelpers;

public class PageTagHelper : TagHelper
{
    public ModalSize Size { get; set; } = ModalSize.Large;
    //<div id="page-container">
    //    <div class="row m-b-sm">
    //        <div class="col-lg-12">
    //        </div>
    //    </div>
    //</div>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "main";
        output.Attributes.SetAttribute("id", "page-container");
        output.Attributes.SetAttribute("class", "py-6 bg-surface-secondary");

        var preSize = Size.IsIn(ModalSize.Medium, ModalSize.Small) ? $"<div class=\"row justify-content-center\"><div class=\"col-xl-{(Size == ModalSize.Small ? "9" : "10")}\">" : "";
        var postSize = Size.IsIn(ModalSize.Medium, ModalSize.Small) ? "</div></div>" : "";

        output.PreContent.SetHtmlContent($"<div class=\"container-fluid\"><div class=\"vstack gap-4\">{preSize}");
        output.PostContent.SetHtmlContent($"{postSize}</div></div>");
    }
}
