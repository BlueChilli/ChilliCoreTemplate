using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class FieldImageUploadWithPreviewOptions : FieldTemplateOptionsBase
    {
        public FieldImageUploadWithPreviewOptions(string imagePath, ImageSharpCommand command, string alternativeImage = null, string buttonText = null)
        {
            this.ImagePath = imagePath;
            this.Command = command;
            this.AlternativeImage = alternativeImage;
            this.ButtonText = buttonText;
        }

        public override string GetViewPath()
        {
            return "FieldTemplates/ImageUploadWithPreview";
        }

        public string ImagePath { get; }
        public ImageSharpCommand Command { get; }

        public string AlternativeImage { get; set;  }

        public string ButtonText { get; set; } = "Upload";
        /// <summary>
        /// Use cover for emulation of crop, contain for emulation of resize
        /// </summary>
        public string BackgroundSize { get; set; } = "cover";

        public bool HasRemoveButton { get; set; }  //The remove button will set a input hidden field #IDRemove to true

        public string RemoveButtonText { get; set; } = "Remove";

        public override IFieldInnerTemplateModel PostProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            templateModel = base.PostProcessInnerField(templateModel);

            HttpPostedFileExtensionsAttribute.Resolve(templateModel.InnerMetadata.ModelMetadata, templateModel.HtmlAttributes);

            return templateModel;
        }
    }
}