using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class PasswordFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public PasswordFieldTemplateOptions() : base() { }
        public PasswordFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Password";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            InputFieldTemplateOptions.ResolveInputAttributes(templateModel, "password");

            return templateModel;
        }
    }
}