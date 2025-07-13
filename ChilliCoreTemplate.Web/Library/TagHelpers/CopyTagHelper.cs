using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers;

//<div><span class="js-copy-text">text to copy</span><i class="js-copy ms-2 bi bi-copy"></i></div>
public class CopyTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.PreContent.AppendHtml("<span class=\"js-copy-text\">");
        output.PostContent.AppendHtml("</span><i class=\"js-copy ms-2 bi bi-copy\"></i>");

        var script = "<script>$('.js-copy').on('click', async function () { var $t = $(this); var text = $t.siblings('span.js-copy-text').text(); try { await navigator.clipboard.writeText(text); $t.addClass('text-success'); setTimeout(function () { $t.removeClass('text-success'); }, 500); } catch (err) { console.error('Failed to copy: ', err) } });</script>";
        CustomScriptsHelper.RegisterCustomScripts(ViewContext, new Guid("c7de5aa6-fa68-4226-a8fc-1a4cddaae84f"), script);
    }
}
