using ChilliSource.Cloud.Web.MVC;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class RadioFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public RadioFieldTemplateOptions() : base() { }
        public RadioFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Radio";
        }

        public RadioAttribute RadioAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var member = templateModel.InnerMetadata.MemberExpression;

            if (this.RadioAttribute == null)
                this.RadioAttribute = member.Member.GetCustomAttribute<RadioAttribute>() ?? new RadioAttribute();

            return templateModel;
        }
    }
}