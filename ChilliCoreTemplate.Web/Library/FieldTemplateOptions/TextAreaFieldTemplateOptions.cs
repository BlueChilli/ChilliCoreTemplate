using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class TextAreaFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public TextAreaFieldTemplateOptions() : base() { }
        public TextAreaFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public int Rows { get; set; } = 4;

        public override string GetViewPath()
        {
            return "FieldTemplates/TextArea";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            InputFieldTemplateOptions.ResolveInputAttributes(templateModel, "text");

            return templateModel;
        }
    }
}