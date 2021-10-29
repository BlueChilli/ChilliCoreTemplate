using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;

namespace ChilliCoreTemplate.Web
{
    public class DateTimePickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public DateTimePickerFieldTemplateOptions() : base() { }
        public DateTimePickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/DateTimePicker";
        }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }
    }
}