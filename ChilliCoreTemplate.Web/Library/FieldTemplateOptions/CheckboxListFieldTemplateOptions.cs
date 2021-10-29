using ChilliSource.Cloud.Web.MVC;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class CheckboxListFieldTemplateOptions : SelectListFieldTemplateOptionsBase
    {
        public CheckboxListFieldTemplateOptions() : base() { }
        public CheckboxListFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/CheckboxList";
        }

        public CheckBoxAttribute CheckboxAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var baseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;

            if (this.CheckboxAttribute == null)
                this.CheckboxAttribute = member.Member.GetCustomAttribute<CheckBoxAttribute>() ?? new CheckBoxAttribute();

            base.ProcessSelect(baseType, metadata, templateModel);

            return templateModel;
        }
    }
}