using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers;

public class ModalTagHelper : TagHelper
{
    public IMvcActionDefinition Menu { get; set; }

    public ModalSize Size { get; set; }


    //div class="modal inmodal fade" id="@Model.Menu.GetModalId()" role="dialog" aria-hidden="true">
    //    <div class="modal-dialog modal-@(Model.Size.GetData<string>("Css"))">
    //        <div class="modal-content">
    //            <div id="@Model.Menu.GetModalId()_content"></div>
    //        </div>
    //    </div>
    //</div>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("modal", NullHtmlEncoder.Create());
        output.AddClass("inmodal", NullHtmlEncoder.Create());
        output.AddClass("fade", NullHtmlEncoder.Create());
        output.Attributes.SetAttribute("id", Menu.GetModalId());
        output.PreContent.SetHtmlContent($"<div class=\"modal-dialog modal-{Size.GetData<string>("Css")}\"><div class=\"modal-content\"><div id=\"{Menu.GetModalId()}_content\"\\>");
        output.PostContent.SetHtmlContent("</div></div></div>");
    }
}

public class ModalHeaderTagHelper : TagHelper
{
    public string Title { get; set; }

    public string Description { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("modal-header", NullHtmlEncoder.Create());
        output.Attributes.Add("style", "display:block");
        //Add close button <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        output.Content.SetHtmlContent($"<h5 class=\"modal-title\">{Title}</h5>{(String.IsNullOrEmpty(Description) ? "" : $"<p>{Description}</p>")}");
    }
}

public class ModalBodyTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("modal-body", NullHtmlEncoder.Create());
    }
}

public class ModalFooterTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("modal-footer", NullHtmlEncoder.Create());
    }
}