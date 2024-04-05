using ChilliSource.Cloud.Web.MVC;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class RadioItemFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public RadioItemFieldTemplateOptions() : base() { }
        public RadioItemFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/RadioItem";
        }

        public RadioItemAttribute RadioItem { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var member = templateModel.InnerMetadata.MemberExpression;

            if (this.RadioItem == null)
                this.RadioItem = member.Member.GetCustomAttribute<RadioItemAttribute>() ?? new RadioItemAttribute();

            return templateModel;
        }
    }
}