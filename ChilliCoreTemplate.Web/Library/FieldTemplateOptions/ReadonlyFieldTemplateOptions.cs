using ChilliSource.Cloud.Web.MVC;
using System;

namespace ChilliCoreTemplate.Web
{
    public class ReadonlyFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public ReadonlyFieldTemplateOptions() : base() { }
        public ReadonlyFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/ReadOnly";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var modelValue = templateModel.InnerMetadata.ModelValue;


            if (!String.IsNullOrEmpty(metadata.DisplayFormatString))
            {
                templateModel.Value = String.Format("{" + metadata.DisplayFormatString + "}", modelValue);
            }

            //select list support ?
            //else if (this.SelectList != null && this.SelectList.Any(x => x.Value == templateModel.Value?.ToString()))
            //{
            //    templateModel.Value = this.SelectList.First(x => x.Value == templateModel.Value.ToString()).Text;
            //}
            //else if (this.SelectList != null && this.SelectList.Any(x => x.Value == templateModel.Value.ToString()))
            //{
            //    templateModel.Value = this.SelectList.First(x => x.Value == templateModel.Value.ToString()).Text;
            //}

            return templateModel;
        }
    }
}