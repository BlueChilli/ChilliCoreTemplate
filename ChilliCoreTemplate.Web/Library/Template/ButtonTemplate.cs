using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static partial class FieldTemplateHelper
    {
        public static Task<IHtmlContent> ButtonAsync(this IHtmlHelper htmlHelper, Template_Button options = null)
        {
            if (options == null) options = new Template_Button();
            options.Type = ButtonType.Button;

            return htmlHelper.TemplateAsync(TemplateTypes.Button, options);
        }

        public static Task<IHtmlContent> ButtonSubmitAsync(this IHtmlHelper htmlHelper, string text)
        {
            return htmlHelper.ButtonSubmitAsync(new Template_Button { Text = text });
        }

        public static Task<IHtmlContent> ButtonSubmitAsync(this IHtmlHelper htmlHelper, Template_Button options = null)
        {
            if (options == null) options = new Template_Button();
            options.Type = ButtonType.Submit;
            if (options.Style == ButtonStyle.Default) options.Style = ButtonStyle.Primary;

            return htmlHelper.TemplateAsync(TemplateTypes.Button, options);
        }

        public static Task<IHtmlContent> LinkAsync(this IHtmlHelper htmlHelper, Template_Button options = null)
        {
            if (options == null) options = new Template_Button();
            options.Type = ButtonType.Link;
            return htmlHelper.TemplateAsync(TemplateTypes.Button, options);
        }
    }

    public class Template_Button
    {
        public Template_Button()
        {
            IsLaddaButton = true;
        }

        public string Text { get; set; }

        public object HtmlAttributes { get; set; }

        public string Url { get; set; }

        public bool IsLaddaButton { get; set; }

        public ButtonType Type { get; set; }

        public ButtonStyle Style { get; set; }

        public bool IsButton { get { return Type == ButtonType.Button || Type == ButtonType.Submit; } }
    }

    public enum ButtonType
    {
        Button = 1,
        Submit,
        Link
    }

    public enum ButtonStyle
    {
        Default,
        Primary,
        Secondary,
        Danger,
        Warning
    }

}