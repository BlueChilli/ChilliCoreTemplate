using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Web
{
    public class FloatingFieldTemplateOptions : InputFieldTemplateOptions
    {
        public bool? IsMandatory { get; set; }

        public override string GetViewPath()
        {
            return "FieldTemplates/Floating";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            base.ProcessInnerField(templateModel);

            if (IsMandatory == null)
            {
                var member = templateModel.InnerMetadata.MemberExpression;

                IsMandatory = member.Member.GetAttribute<RequiredAttribute>(false) != null;
            }

            return templateModel;
        }
    }
}