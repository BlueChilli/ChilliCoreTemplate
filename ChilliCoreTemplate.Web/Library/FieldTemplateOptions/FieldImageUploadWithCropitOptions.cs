using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class FieldImageUploadWithCropitOptions : FieldTemplateOptionsBase
    {
        public FieldImageUploadWithCropitOptions(string form, string imagePath, int widthInPixels, int heightInPixels, string alternativeImage = null, string buttonText = "Upload")
        {
            this.Form = form;
            this.WidthInPixels = widthInPixels;
            this.HeightInPixels = heightInPixels;
            this.ImagePath = imagePath;
            this.AlternativeImage = alternativeImage;
            this.ButtonText = buttonText;
        }

        public override string GetViewPath()
        {
            return "FieldTemplates/ImageUploadWithCropit";
        }

        public string Form { get; set; }

        public int WidthInPixels { get; set; }
        public int HeightInPixels { get; set; }

        public string ImagePath { get; }

        public string AlternativeImage { get; set;  }

        public string ButtonText { get; set; }

        public string RemoveButtonText { get; set; } = "Remove";


        public override IFieldInnerTemplateModel PostProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            templateModel = base.PostProcessInnerField(templateModel);

            HttpPostedFileExtensionsAttribute.Resolve(templateModel.InnerMetadata.ModelMetadata, templateModel.HtmlAttributes);

            return templateModel;
        }
    }
}