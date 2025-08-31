using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers;

public class OffCanvasTagHelper : TagHelper
{
    public IMvcActionDefinition Menu { get; set; }

    public ModalSize Size { get; set; }


    //<div class="offcanvas offcanvas-end offcanvas-size-xl" tabindex="-1" id="@Model.Menu.GetModalId()">
    //    <div id="@Model.Menu.GetModalId()_content" class="d-flex flex-column h-full"></div>
    //</div>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("offcanvas", NullHtmlEncoder.Create());
        output.AddClass("offcanvas-end", NullHtmlEncoder.Create());
        output.AddClass($"offcanvas-size-{Size.GetData<string>("Css")}", NullHtmlEncoder.Create());
        output.Attributes.SetAttribute("id", Menu.GetModalId());
        output.PreContent.SetHtmlContent($"<div id=\"{Menu.GetModalId()}_content\" class=\"d-flex flex-column h-full\"></div>");
    }
}

public class OffCanvasHeaderTagHelper : TagHelper
{
    public string Title { get; set; }

    public string Description { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("offcanvas-header", NullHtmlEncoder.Create());
        output.AddClass("border-bottom", NullHtmlEncoder.Create());
        output.AddClass("offcanvas-header", NullHtmlEncoder.Create());
        output.AddClass("bg-surface-secondary", NullHtmlEncoder.Create());
        output.PreContent.SetHtmlContent($"<h5 class=\"offcanvas-title\">{Title}</h5>");
        output.PostContent.SetHtmlContent("<button type=\"button\" class=\"btn-close text-reset\" data-bs-dismiss=\"offcanvas\" aria-label=\"Close\"></button>");
    }
}

public class OffCanvasBodyTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("offcanvas-body", NullHtmlEncoder.Create());
    }
}

public class OffCanvasFooterTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("d-flex", NullHtmlEncoder.Create());
        output.AddClass("p-4", NullHtmlEncoder.Create());
        output.AddClass("bg-surface-secondary", NullHtmlEncoder.Create());
        output.AddClass("w-full", NullHtmlEncoder.Create());
    }
}