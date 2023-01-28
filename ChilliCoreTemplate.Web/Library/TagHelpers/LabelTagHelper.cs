using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{
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
}
