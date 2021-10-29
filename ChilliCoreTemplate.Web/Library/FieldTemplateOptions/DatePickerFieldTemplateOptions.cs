using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;

namespace ChilliCoreTemplate.Web
{
    public class DatePickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public DatePickerFieldTemplateOptions() : base() { }
        public DatePickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/DatePicker";
        }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }
    }
}