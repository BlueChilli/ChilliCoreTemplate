using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    /// <summary>
    /// https://getbootstrap.com/docs/5.0/components/dropdowns/
    /// </summary>
    [HtmlTargetElement("button-dropdown")]
    [RestrictChildren("item", "itempost", "item-divider")]
    public class ButtonDropdownTagHelper : TagHelper
    {
        public string Title { get; set; }

        public IconType Icon { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("dropdown", NullHtmlEncoder.Create());

            var icon = String.Empty;
            if (Icon != IconType.None)
            {
                icon = $"<span class=\"pe-3\"><i class=\"bi bi-{Icon.GetData<string>("Icon")}\"></i></span>";
            }

            var button = $"<button class=\"btn btn-sm btn-neutral dropdown-toggle\" type=\"button\" data-bs-toggle=\"dropdown\">{icon}{Title}</button>";
            var list = $"<ul class=\"dropdown-menu dropdown-menu-xs\">";

            output.PreContent.SetHtmlContent(button + list);
            output.PostContent.SetHtmlContent("</ul>");
        }
    }

    [HtmlTargetElement("item", ParentTag= "button-dropdown", Attributes = ActionAttribute)]
    public class ButtonDropdownItemModalTagHelper : AnchorModalTagHelper
    {
        public ButtonDropdownItemModalTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.AddClass("dropdown-item", NullHtmlEncoder.Create());
            base.Process(context, output);
            output.PreElement.SetHtmlContent("<li>");
            output.PostElement.SetHtmlContent("</li>");
        }
    }

    [HtmlTargetElement("item", ParentTag = "button-dropdown", Attributes = ActionAttribute)]
    public class ButtonDropdownItemTagHelper : AnchorTagHelper
    {
        public ButtonDropdownItemTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.AddClass("dropdown-item", NullHtmlEncoder.Create());
            base.Process(context, output);
            output.PreElement.SetHtmlContent("<li>");
            output.PostElement.SetHtmlContent("</li>");
        }
    }

    [HtmlTargetElement("item", ParentTag = "button-dropdown", Attributes = ActionAttribute)]
    public class ButtonDropdownItemOffCanvasTagHelper : AnchorOffCanvasTagHelper
    {
        public ButtonDropdownItemOffCanvasTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.AddClass("dropdown-item", NullHtmlEncoder.Create());
            base.Process(context, output);
            output.PreElement.SetHtmlContent("<li>");
            output.PostElement.SetHtmlContent("</li>");
        }
    }

    [HtmlTargetElement("itempost", ParentTag = "button-dropdown")]
    public class ButtonDropdownItemPostTagHelper : ButtonPostTagHelper
    {
        public ButtonDropdownItemPostTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.AddClass("dropdown-item", NullHtmlEncoder.Create());
            base.Process(context, output);
            output.PreElement.SetHtmlContent("<li>");
            output.PostElement.SetHtmlContent("</li>");
        }
    }

    [HtmlTargetElement("item-divider", ParentTag = "button-dropdown")]
    public class ButtonDropdownItemDividerTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "hr";
            output.TagMode = TagMode.StartTagOnly;
            output.AddClass("dropdown-divider", NullHtmlEncoder.Create());
            base.Process(context, output);
            output.PreElement.SetHtmlContent("<li>");
            output.PostElement.SetHtmlContent("</li>");
        }
    }
}
