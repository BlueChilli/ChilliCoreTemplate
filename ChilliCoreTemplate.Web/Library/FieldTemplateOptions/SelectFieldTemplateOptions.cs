using ChilliSource.Cloud.Web.MVC;

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
            var baseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;
            base.ProcessSelect(baseType, metadata, templateModel);

            return templateModel;
        }
    }
}