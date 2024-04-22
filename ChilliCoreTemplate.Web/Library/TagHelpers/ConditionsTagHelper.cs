using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class ConditionsTagHelper : TagHelper
    {
        //<ol class="nested-counter-list">
        //    <li>
        //        <strong></strong> and is subject to the following provisions:
        //        <ol class="nested-counter-list">
        //            <li></li>
        //            <li></li>
        //            <li></li>
        //            <li></li>
        //        </ol>
        //    </li>
        //    <li>
        //        <strong></strong>
        //        <ol class="nested-counter-list">
        //            <li>some text:
        //                <ol>
        //                    <li></li>
        //                    <li></li>
        //                </ol>
        //            </li>
        //            <li></li>
        //        </ol>
        //    </li>
        //    <li>
        //        <strong></strong>
        //        <ol class="nested-counter-list">
        //            <li></li>
        //            <li></li>
        //            <li></li>
        //        </ol>
        //    </li>
        //</ol>

        public ConditionModel Data { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "";
            var sb = new StringBuilder();
            ProcessCondition(sb, Data, 0);
            output.Content.SetHtmlContent(new HtmlString(sb.ToString()));
        }

        private void ProcessCondition(StringBuilder sb, ConditionModel condition, int level)
        {
            if (condition.Conditions != null && condition.Conditions.Count != 0)
            {
                if (condition.Content != null) sb.AppendLine($"<li>{condition.Content}");
                sb.AppendLine($"<ol class=\"nested-counter-list{(level > 0 ? "-level" + level : "")}\">");
                foreach (var subcondition in condition.Conditions) ProcessCondition(sb, subcondition, level + 1);
                sb.AppendLine("</ol>");
                if (condition.Content != null) sb.AppendLine("</li>");
            }
            else
            {
                sb.AppendLine($"<li>{condition.Content}</li>");
            }
        }
    }
}
