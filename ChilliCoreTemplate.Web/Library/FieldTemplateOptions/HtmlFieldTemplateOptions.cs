using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web
{
    public class HtmlFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public HtmlFieldTemplateOptions() : base() { }
        public HtmlFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Html";
        }

        public List<KeyValuePair<string, List<string>>> Toolbar { get; set; }
                = new List<KeyValuePair<string, List<string>>>
                {
                    new KeyValuePair<string, List<string>>("group1", new List<string> { "bold", "link" })
                };

        public bool IsInModal { get; set; }

        public IHtmlContent ToolbarJson()
        {
            var items = Toolbar.Select(g => $"['{g.Key}', [{g.Value.Select(i => $"'{i}'").ToDelimitedString(",")}]]").ToList();
            return MvcHtmlStringCompatibility.Create($"[{items.ToDelimitedString(",")}]");
        }

        public IMvcActionDefinition ImageUpload { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            InputFieldTemplateOptions.ResolveInputAttributes(templateModel, "hidden");

            return templateModel;
        }
    }
}