using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class FileFieldTemplateOptions : FieldTemplateOptionsBase
    {        
        public FileFieldTemplateOptions() : base() { }
        public FileFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public string ButtonText { get; set; } = "Choose";

        public override string GetViewPath()
        {
            return "FieldTemplates/File";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            HttpPostedFileExtensionsAttribute.Resolve(metadata, templateModel.HtmlAttributes);

            return templateModel;
        }
    }
}