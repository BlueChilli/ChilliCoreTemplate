using ChilliSource.Cloud.Web.MVC;
using System;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class DateFieldTemplateOptions : FieldTemplateOptionsBase
    {        
        public DateFieldTemplateOptions() : base() { }
        public DateFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Date";
        }

        public DateFormatAttribute DateFormatAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;

            if (templateModel.Value is String)
            {
                var dateParts = ((string)templateModel.Value).Split(',');
                if (dateParts.Length >= 2 && !String.IsNullOrEmpty(dateParts[0]) && !String.IsNullOrEmpty(dateParts[1]))
                {
                    if (dateParts.Length == 2)
                    {
                        templateModel.Value = new DateTime(int.Parse(dateParts[1]), int.Parse(dateParts[0]), 1);
                    }
                    else
                    {
                        templateModel.Value = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                    }
                }
                else
                {
                    DateTime d;
                    if (!String.IsNullOrEmpty(metadata.DisplayFormatString) && DateTime.TryParseExact((string)templateModel.Value, metadata.DisplayFormatString, null, System.Globalization.DateTimeStyles.None, out d))
                    {
                        templateModel.Value = d;
                    }
                    else if (DateTime.TryParse((string)templateModel.Value, out d))
                    {
                        templateModel.Value = d;
                    }
                    else
                    {
                        templateModel.Value = null;
                    }
                }
            }

            if (this.DateFormatAttribute == null)
            {
                var dateFormatAttribute = member.Member.GetCustomAttribute<DateFormatAttribute>();
                if (dateFormatAttribute == null)
                {
                    return templateModel.UseOptions(new DatePickerFieldTemplateOptions(this));
                }
                else
                {
                    this.DateFormatAttribute = dateFormatAttribute;
                }
            }

            return templateModel;
        }
    }
}