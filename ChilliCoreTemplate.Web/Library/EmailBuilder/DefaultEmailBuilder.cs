using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Html;
using System.IO;

namespace ChilliCoreTemplate.Web
{
    public static class EmailConstants
    {
        public static readonly DefaultEmailBuilderOptions EmailOptions;

        static EmailConstants()
        {
            EmailOptions = new DefaultEmailBuilderOptions();
            EmailOptions.ButtonStyle.BackGroundColor = "#FF7043";
            EmailOptions.ButtonStyle.BorderColor = "#FF7043";
            EmailOptions.H1Style.TextColor = "#393D3F";
            EmailOptions.H2Style.TextColor = "#393D3F";
            EmailOptions.H3Style.TextColor = "#393D3F";
            EmailOptions.PStyle.TextColor = "#757575";
        }
    }
    /// <summary>
    /// Email builder options
    /// </summary>
    public class DefaultEmailBuilderOptions
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultEmailBuilderOptions()
        {
            this.ButtonStyle = new Button_Style()
            {
                TextColor = "white",
                BackGroundColor = "#722255",
                BorderColor = "#571a41"
            };

            this.H1Style = new TextStyle() { TextColor = "#384047" };
            this.H2Style = new TextStyle() { TextColor = "#8d9aa5" };
            this.H3Style = new TextStyle() { TextColor = "#384047" };
            this.H4Style = new TextStyle() { TextColor = "#8d9aa5" };

            this.PStyle = new TextStyle() { TextColor = "#8d9aa5" };
        }

        /// <summary>
        /// Button styles
        /// </summary>
        public Button_Style ButtonStyle { get; private set; }

        public TextStyle H1Style { get; private set; }
        public TextStyle H2Style { get; private set; }
        public TextStyle H3Style { get; private set; }
        public TextStyle H4Style { get; private set; }
        public TextStyle PStyle { get; private set; }

        /// <summary>
        /// Button styles
        /// </summary>
        public class Button_Style
        {
            /// <summary>
            /// TextColor
            /// </summary>
            public string TextColor { get; set; }

            /// <summary>
            /// BackGroundColor
            /// </summary>
            public string BackGroundColor { get; set; }

            /// <summary>
            /// BorderColor
            /// </summary>
            public string BorderColor { get; set; }

            /// <summary>
            /// If true button is wrapped with a div with text-align:center
            /// </summary>
            public bool IsCentered { get; internal set; }
        }

        public class TextStyle
        {
            public string TextColor { get; set; }
        }
    }

    /// <summary>
    /// Email Builder Helper. Use EmailBuilder.CreateDefault to instantiate this class
    /// </summary>
    public class DefaultEmailBuilder
    {
        private static readonly Dictionary<string, string> EmailTemplateValues = new Dictionary<string, string>()
        {
            { "H1Begin", @"<h1 style=""font-family:Open Sans,Helvetica,sans-serif;color:{0};display:block;font-size:24px;font-weight:bold;line-height:130%;letter-spacing:normal;margin-right:0;margin-top:15px;margin-bottom:15px;margin-left:0;text-align:left;"">" },
            { "H1End", "</h1>" },
            { "H2Begin", @"<h2 style=""font-family:Open Sans,Helvetica,sans-serif;color:{0};display:block;font-size:18px;font-weight:normal;line-height:150%;letter-spacing:normal;margin-right:0;margin-top:15px;margin-bottom:6px;margin-left:0;text-align:left;"">" },
            { "H2End", @"</h2>" },
            { "H3Begin", @"<h3 style=""font-family:Open Sans,Helvetica,sans-serif;color:{0};display:block;font-size:18px;font-weight:bold;line-height:130%;letter-spacing:normal;margin-right:0;margin-left:0;margin-top:15px;margin-bottom:5px;text-align:left;"">" },
            { "H3End", @"</h3>" },
            { "H4Begin", @"<h4 style=""font-family:Open Sans,Helvetica,sans-serif;color:{0};display:block;font-size:14px;font-weight:normal;line-height:130%;letter-spacing:normal;margin-right:0;margin-left:0;margin-top:2px;margin-bottom:15px;text-align:left;"">" },
            { "H4End", @"</h4>" },
            { "PBegin", @"<p style=""font-family:Open Sans,Helvetica,sans-serif;line-height:150%;margin-top:15px;margin-bottom:15px;color:{0};"">" },
            { "PEnd", @"</p>" }
        };

        IHtmlHelper HtmlHelper;
        DefaultEmailBuilderOptions Options;

        internal DefaultEmailBuilder(IHtmlHelper htmlHelper, DefaultEmailBuilderOptions options)
        {
            this.HtmlHelper = htmlHelper;
            this.Options = options;
        }

        public IDisposable BeginRow(RowBorderType borderType = RowBorderType.None, RowType rowType = RowType.Normal, RowPaddingType rowPaddingType = RowPaddingType.Normal, RowAlignment rowAlignment = RowAlignment.Left)
        {
            return new EmailRowContainer(HtmlHelper, borderType, rowType, rowPaddingType, rowAlignment);
        }

        public IDisposable BeginColumn()
        {
            return new EmailColumnContainer(HtmlHelper);
        }

        public IDisposable BeginNormalColumn()
        {
            return new EmailNormalColumnContainer(HtmlHelper);
        }

        public IDisposable BeginHeader(RowPaddingType rowPaddingType = RowPaddingType.Normal)
        {
            return new EmailHeaderContainer(HtmlHelper, rowPaddingType);
        }

        public IDisposable BeginP()
        {
            return new EnclosedTagContainer(HtmlHelper, "P", Options.PStyle);
        }

        public IDisposable BeginH1()
        {
            return new EnclosedTagContainer(HtmlHelper, "H1", Options.H1Style);
        }

        public IDisposable BeginH2()
        {
            return new EnclosedTagContainer(HtmlHelper, "H2", Options.H2Style);
        }

        public IDisposable BeginH3()
        {
            return new EnclosedTagContainer(HtmlHelper, "H3", Options.H3Style);
        }

        public IDisposable BeginH4()
        {
            return new EnclosedTagContainer(HtmlHelper, "H4", Options.H4Style);
        }

        public IHtmlContent H1(string html)
        {
            return RenderTag(html, "H1", Options.H1Style);
        }

        public IHtmlContent H2(string html)
        {
            return RenderTag(html, "H2", Options.H2Style);
        }

        public IHtmlContent H3(string html)
        {
            return RenderTag(html, "H3", Options.H3Style);
        }

        public IHtmlContent H4(string html)
        {
            return RenderTag(html, "H4", Options.H4Style);
        }

        public IHtmlContent P(string html)
        {
            return RenderTag(html, "P", Options.PStyle);
        }

        private IHtmlContent RenderTag(string html, string tagName, DefaultEmailBuilderOptions.TextStyle textStyle)
        {
            var value = EmailTemplateValues[string.Format("{0}Begin", tagName)];
            value = string.Format(value, textStyle.TextColor);
            return new HtmlString(string.Format("{0}{1}{2}", value, html, EmailTemplateValues[string.Format("{0}End", tagName)]));
        }

        private string EmailButton => File.ReadAllText("Library/Resources/EmailButton.html");
        public IHtmlContent Button(string url, string title, DefaultEmailBuilderOptions.Button_Style buttonStyle = null)
        {
            if (buttonStyle == null) buttonStyle = Options.ButtonStyle;

            var button = string.Format(EmailButton, buttonStyle.BackGroundColor, buttonStyle.TextColor, url, title);
            if (buttonStyle.IsCentered)
            {
                button = $"<div style=\"text-align:center\">{button}</div>";
            }
            return new HtmlString(button);
        }

        public IHtmlContent ButtonTrack(ShortGuid emailId, string templateId, string url, string title, DefaultEmailBuilderOptions.Button_Style buttonStyle = null)
        {
            if (buttonStyle == null) buttonStyle = Options.ButtonStyle;

            var urlHelper = this.HtmlHelper.GetUrlHelper();
            url = Mvc.Root.EmailAccount_EmailRedirect.Url(urlHelper, routeValues: new { EmailId = emailId, Url = url, utm_medium = "Email", utm_source = templateId }, protocol: "https");

            return Button(url, title, buttonStyle);
        }

        public IHtmlContent ImageRow(string imageUrl, string url, string alt = null)
        {
            var altText = string.IsNullOrWhiteSpace(alt) ? string.Empty : string.Format(@"alt=""{0}""", alt);
            return new HtmlString(string.Format(@"<td style=""vertical-align:top;color:#8d9aa5;font-size:14px;text-align:center;border-collapse:collapse;margin:0;padding:0;background:#fff;""><a style=""margin:0;padding:0;"" href=""{0}"" target=""_blank""><img id=""full"" style=""width:100%;max-width:600px;height:auto;border:0;line-height:100%;outline:none;text-decoration:none;text-align:center;margin:0;padding:0;"" {1} src=""{2}""></a></td>", url, altText, imageUrl));
        }

        public IHtmlContent ImageRow(string imageUrl, string alt = null)
        {
            var altText = string.IsNullOrWhiteSpace(alt) ? string.Empty : string.Format(@"alt=""{0}""", alt);
            return new HtmlString(string.Format(@"<td style=""vertical-align:top;color:#8d9aa5;font-size:14px;text-align:center;border-collapse:collapse;margin:0;padding:0;background:#fff;""><img id=""full"" style=""width:100%;max-width:600px;height:auto;border:0;line-height:100%;outline:none;text-decoration:none;text-align:center;margin:0;padding:0;"" {0} src=""{1}""></td>", altText, imageUrl));
        }

        public IHtmlContent ShortImageColumn(string imageUrl, string url, string alt = null)
        {
            var altText = string.IsNullOrWhiteSpace(alt) ? string.Empty : string.Format(@"alt=""{0}""", alt);
            return new HtmlString(string.Format(@"<td style=""vertical-align:top;width:85px;color:#8d9aa5;font-size:15px;text-align:left;padding-top:15px;padding-right:25px;border-collapse:collapse;""><div style=""width:85px;height:85px;background:#FFF;text-align:right;""><a href=""{0}"" target=""_blank""><img width=""78"" height=""85"" alt=""{1}"" style=""width:85px;height:85px;display:block;border:0; height:auto;line-height:100%;outline:none;text-decoration:none;"" src=""{2}""></a></div></td>", url, altText, imageUrl));
        }

        public enum RowBorderType
        {
            [Data("Style", "")]
            None,
            [Data("Style", "border-top: 1px solid #e2e5e8;")]
            Top,
            [Data("Style", "border-bottom: 1px solid #e2e5e8;")]
            Bottom
        }

        public enum RowType
        {
            Normal,
            TwoColumn
        }

        public enum RowAlignment
        {
            [Data("Alignment", "left")]
            Left,
            [Data("Alignment", "center")]
            Center
        }

        public enum RowPaddingType
        {
            [Data("CellPaddingTop", "15px")]
            [Data("RowPaddingBottom", "3%")]
            [Data("HeaderPaddingBottom", "25px")]
            Normal,
            [Data("CellPaddingTop", "0")]
            [Data("RowPaddingBottom", "0")]
            [Data("HeaderPaddingBottom", "0")]
            Reduced
        }

        class EmailHeaderContainer : IDisposable
        {
            RowPaddingType RowPaddingType;
            private DisposableWrapper _disposableWrapper;

            public EmailHeaderContainer(IHtmlHelper htmlHelper, RowPaddingType rowPaddingType)
            {
                this.RowPaddingType = rowPaddingType;
                _disposableWrapper = new DisposableWrapper(htmlHelper, this.Begin, this.End);
            }

            protected IHtmlContent Begin()
            {
                return new HtmlString(string.Format(@"<table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 100%;"" width=""100%""><tr><td style=""vertical-align:top;font-size:18px;font-weight:bold;line-height:100%;padding-bottom:{0};text-align:left;border-collapse:collapse;"">", this.RowPaddingType.GetData<string>("HeaderPaddingBottom")));
            }

            public IHtmlContent End()
            {
                return new HtmlString(@"</td></tr></table>");
            }

            public void Dispose()
            {
                _disposableWrapper.Dispose();
            }
        }

        class EmailRowContainer : IDisposable
        {
            private RowBorderType BorderType;
            private RowType RowType;
            private RowPaddingType RowPaddingType;
            private RowAlignment RowAlignment;

            private DisposableWrapper _disposableWrapper;
            public EmailRowContainer(IHtmlHelper htmlHelper, RowBorderType borderType, RowType rowType, RowPaddingType rowPaddingType, RowAlignment rowAlignment)
            {
                this.BorderType = borderType;
                this.RowType = rowType;
                this.RowPaddingType = rowPaddingType;
                this.RowAlignment = rowAlignment;

                _disposableWrapper = new DisposableWrapper(htmlHelper, this.BeginRow, this.EndRow);
            }

            protected IHtmlContent BeginRow()
            {
                var alignment = this.RowAlignment.GetData<string>("Alignment");
                var defaultSuffix = this.RowType == RowType.Normal ? String.Format(@"<tr><td style=""vertical-align:top;color:#8d9aa5;font-size:15px;line-height:150%;text-align:{2};border-collapse:collapse;margin:0;padding:0; {0} padding-top:{1};"">", this.BorderType.GetData<string>("Style"), this.RowPaddingType.GetData<string>("CellPaddingTop"), alignment) : string.Empty;
                return new HtmlString(String.Format(@"<tr><td align=""{2}"" style=""vertical-align:top;border-collapse:collapse;padding-right:7%;padding-left:7%;padding-bottom:{1};""><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""width: 100%;"">{0}", defaultSuffix, this.RowPaddingType.GetData<string>("RowPaddingBottom"), alignment));
            }

            public IHtmlContent EndRow()
            {
                var defaultPrefix = this.RowType == RowType.Normal ? @"</td></tr>" : string.Empty;
                return new HtmlString(String.Format(@"{0}</table></td></tr>", defaultPrefix));
            }

            public void Dispose()
            {
                _disposableWrapper.Dispose();
            }
        }

        class EmailColumnContainer : IDisposable
        {
            private DisposableWrapper _disposableWrapper;

            public EmailColumnContainer(IHtmlHelper htmlHelper)
            {
                _disposableWrapper = new DisposableWrapper(htmlHelper, this.Begin, this.End);
            }

            protected IHtmlContent Begin()
            {
                return new HtmlString(@"<tr>");
            }

            public IHtmlContent End()
            {
                return new HtmlString(@"</tr>");
            }

            public void Dispose()
            {
                _disposableWrapper.Dispose();
            }
        }

        class EmailNormalColumnContainer : IDisposable
        {
            private DisposableWrapper _disposableWrapper;

            public EmailNormalColumnContainer(IHtmlHelper htmlHelper)
            {
                _disposableWrapper = new DisposableWrapper(htmlHelper, this.Begin, this.End);
            }

            protected IHtmlContent Begin()
            {
                return new HtmlString(@"<td style=""vertical-align:top;color:#8d9aa5;font-size:15px;line-height:150%;text-align:left;border-collapse:collapse;"">");
            }

            public IHtmlContent End()
            {
                return new HtmlString(@"</td>");
            }

            public void Dispose()
            {
                _disposableWrapper.Dispose();
            }
        }

        internal class EnclosedTagContainer : IDisposable
        {
            private string TagName;
            private DefaultEmailBuilderOptions.TextStyle TextStyle;
            private DisposableWrapper _disposableWrapper;

            public EnclosedTagContainer(IHtmlHelper htmlHelper, string tagName, DefaultEmailBuilderOptions.TextStyle textStyle)
            {
                this.TagName = tagName;
                this.TextStyle = textStyle;
                _disposableWrapper = new DisposableWrapper(htmlHelper, this.Begin, this.End);
            }

            protected IHtmlContent Begin()
            {
                var value = EmailTemplateValues[string.Format("{0}Begin", this.TagName)];
                value = string.Format(value, TextStyle.TextColor);
                return new HtmlString(value);
            }

            public IHtmlContent End()
            {
                return new HtmlString(EmailTemplateValues[string.Format("{0}End", this.TagName)]);
            }

            public void Dispose()
            {
                _disposableWrapper.Dispose();
            }
        }
    }
}