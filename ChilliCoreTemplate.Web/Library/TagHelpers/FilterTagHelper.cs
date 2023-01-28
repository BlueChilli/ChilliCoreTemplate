using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{

    //<div class="d-flex flex-row gap-20">
    //    <div class="d-flex gap-3">
    //    </div>
    //    <div class="d-flex flex-md-grow-1">
    //    </div>
    //</div>

    public class FilterTagHelper : TagHelper
    {

        public FieldTemplateSize Size { get; set; } = FieldTemplateSize.Medium;

        private int Gap()
        {
            switch (Size)
            {
                case FieldTemplateSize.ExtraLarge: return 50;
                case FieldTemplateSize.Large: return 30;
                case FieldTemplateSize.Medium: return 20;
                case FieldTemplateSize.Small: return 10;
                case FieldTemplateSize.ExtraSmall: return 5;
            }
            return 0;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"d-flex flex-row gap-{Gap()}");
        }
    }

    public class FilterLeftTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "d-flex gap-3");
        }
    }

    public class FilterRightTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "d-flex flex-md-grow-1");
        }
    }
}
