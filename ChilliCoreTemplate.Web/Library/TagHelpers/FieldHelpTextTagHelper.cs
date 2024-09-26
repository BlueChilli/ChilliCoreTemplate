using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public class FieldHelpTextTagHelper : TagHelper
    {
        public string HelpText { get; set; }

        public bool IsToolTip { get; set; } //Will need to initialise tooltips

        //Standard = <small><br>@Html.Raw(Model.HelpText)</small>
        //Tooltip = <i class="bi bi-question-circle-fill" data-bs-toggle="tooltip" data-bs-original-title="@stat.Tooltip" data-bs-html="true"></icon>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (String.IsNullOrEmpty(HelpText))
            {
                output.TagName = null;
                return;
            }
            if (IsToolTip)
            {
                output.TagName = "i";
                output.Attributes.SetAttribute("class", "bi bi-question-circle-fill");
                output.Attributes.Add("type", $"bi bi-question-circle-fill");
                output.Attributes.Add("data-bs-toggle", "tooltip");
                output.Attributes.Add("data-bs-original-title", HelpText);
                output.Attributes.Add("data-bs-html", "true");
            }
            else
            {
                output.TagName = "small";
                output.PreContent.SetHtmlContent("<br>");
                output.Content.SetHtmlContent(HelpText);
            }
        }
    }
}
