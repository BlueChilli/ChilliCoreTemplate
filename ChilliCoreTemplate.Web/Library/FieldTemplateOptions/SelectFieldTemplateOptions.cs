using ChilliSource.Cloud.Web.MVC;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class SelectFieldTemplateOptions : SelectListFieldTemplateOptionsBase
    {
        public SelectFieldTemplateOptions() : base() { }
        public SelectFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Select";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var baseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;

            var isRequired = member.Member.GetCustomAttribute<RequiredAttribute>() != null;

            base.ProcessSelect(baseType, metadata, templateModel, isRequired);

            if (isRequired && !templateModel.HtmlAttributes.ContainsKey("required"))
                templateModel.HtmlAttributes.Add("required", "required");

            return templateModel;
        }
    }
}