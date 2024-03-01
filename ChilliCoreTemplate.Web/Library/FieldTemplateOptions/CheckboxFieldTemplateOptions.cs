using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class CheckboxFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public CheckboxFieldTemplateOptions() : base() { }
        public CheckboxFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Checkbox";
        }

        public CheckBoxAttribute CheckBoxAttribute { get; set; }

        public bool IsFloating { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var member = templateModel.InnerMetadata.MemberExpression;

            if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(templateModel.Value))
            {
                templateModel.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
            }
            templateModel.HtmlAttributes.AddOrSkipIfExists("type", "checkbox");

            if (this.CheckBoxAttribute == null)
                this.CheckBoxAttribute = member.Member.GetCustomAttribute<CheckBoxAttribute>();

            return templateModel;
        }
    }
}