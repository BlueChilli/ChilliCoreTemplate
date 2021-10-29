using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class ClockPickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public ClockPickerFieldTemplateOptions() : base() { }
        public ClockPickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/ClockPicker";
        }
    }
}