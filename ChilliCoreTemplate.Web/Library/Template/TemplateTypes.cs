using ChilliSource.Cloud.Web.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class TemplateTypes
    {
        public static readonly TemplateType Button = new TemplateType("Templates/Button");
        public static readonly TemplateType PageButtons = new TemplateType("Templates/PageButtons");
        public static readonly TemplateType PageContainerLeft = new TemplateType("Templates/PageContainerLeft");
        public static readonly TemplateType PageContainerTop = new TemplateType("Templates/PageContainerTop");
        public static readonly TemplateType PageMessage = new TemplateType("Templates/PageMessage");
        public static readonly TemplateType PageHeader = new TemplateType("Templates/PageHeader");
        public static readonly TemplateType ValidationSummary = new TemplateType("Templates/ValidationSummary");
        public static readonly TemplateType GoogleAnalytics = new TemplateType("Templates/GoogleAnalytics");
    }

    public class FieldTemplateLayouts
    {
        public static readonly FieldTemplateLayout StandardField = new FieldTemplateLayout("FieldTemplateLayouts/StandardField");
        public static readonly FieldTemplateLayout VerticalField = new FieldTemplateLayout("FieldTemplateLayouts/VerticalField");
        public static readonly FieldTemplateLayout OptionalField = new FieldTemplateLayout("FieldTemplateLayouts/OptionalField");
        public static readonly FieldTemplateLayout ModalField = new FieldTemplateLayout("FieldTemplateLayouts/ModalField");
        public static readonly FieldTemplateLayout HiddenField = new FieldTemplateLayout("FieldTemplateLayouts/HiddenField");
        public static readonly FieldTemplateLayout FloatingField = new FieldTemplateLayout("FieldTemplateLayouts/FloatingField");
    }
}
