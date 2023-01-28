using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace ChilliCoreTemplate.Web
{
    public class FieldTemplateOptions : FieldTemplateOptionsBase
    {
        public FieldTemplateOptions() { }

        public static IHtmlContent AddOn(string text, string classes = "")
        {
            return new HtmlString($"<span class=\"input-group-text {classes}\">{text}</span>");
        }

        public static IHtmlContent ButtonAddOn(string text, string classes = "")
        {
            return new HtmlString($"<span class=\"input-group-btn\"><button type=\"button\" class=\"btn {classes}\">{text}</button></span>");
        }

        public static IHtmlContent IconAddOn(string type)
        {
            return new HtmlString($"<span class=\"input-group-text\"><i class=\"bi bi-{type}\"></i></span>");
        }

        public override IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var templateModel = base.CreateFieldInnerTemplateModel(html, expression);

            var valueTypeName = templateModel.InnerMetadata.MemberUnderlyingType.Name;
            var valueBaseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var defaultMetaData = (DefaultModelMetadata)metadata;
            var isReadOnly = (defaultMetaData?.Attributes?.PropertyAttributes?.Any(x => x.GetType() == typeof(ReadOnlyAttribute)) ?? false);

            if (isReadOnly)
            {
                templateModel = templateModel.UseOptions(new ReadonlyFieldTemplateOptions(this));
            }
            else if (metadata.AdditionalValues.ContainsKey("CheckBox"))
            {
                templateModel = templateModel.UseOptions(new CheckboxFieldTemplateOptions(this));
            }
            else if (metadata.AdditionalValues.ContainsKey("Radio"))
            {
                if (valueBaseType == typeof(Enum))
                {
                    templateModel = templateModel.UseOptions(new RadioListFieldTemplateOptions(this));
                }
                else
                {
                    templateModel = templateModel.UseOptions(new RadioFieldTemplateOptions(this));
                }
            }
            else if (valueBaseType == typeof(Enum))
            {
                templateModel = templateModel.UseOptions(new SelectFieldTemplateOptions(this));
            }
            else if (valueTypeName == "DateTime")
            {
                templateModel = templateModel.UseOptions(new DateFieldTemplateOptions(this));
            }
            else if (valueTypeName == "Int32")
            {
                var minMax = IntegerDropDownAttribute.ResolveAttribute(metadata, member);
                if (minMax != null)
                {
                    var selectedValue = templateModel.Value.ToNullable<int>();
                    int min = minMax.Min;
                    int max = minMax.Max;
                    var intList = Enumerable.Range(min, max - min + 1).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = selectedValue.HasValue && selectedValue.Value == i }).ToList();
                    if (minMax.IsReverse)
                        intList.Reverse();

                    var selectOptions = new SelectFieldTemplateOptions(this)
                    {
                        SelectList = intList
                    };

                    templateModel = templateModel.UseOptions(selectOptions);
                }
                else
                {
                    templateModel = templateModel.UseOptions(new InputFieldTemplateOptions(this));
                }
            }
            else if (valueTypeName == "HttpPostedFileBase" || valueTypeName == "IFormFile")
            {
                templateModel = templateModel.UseOptions(new FileFieldTemplateOptions(this));
            }
            else if (valueTypeName == "String" && metadata.DataTypeName == "MultilineText")
            {
                templateModel = templateModel.UseOptions(new TextAreaFieldTemplateOptions(this));
            }
            else if (valueTypeName == "String" && metadata.DataTypeName == "Html")
            {
                templateModel = templateModel.UseOptions(new HtmlFieldTemplateOptions(this));
            }
            else
            {
                templateModel = templateModel.UseOptions(new InputFieldTemplateOptions(this));
            }

            return templateModel;
        }

        public override string GetViewPath()
        {
            throw new NotSupportedException("GetViewPath method is not supported in the default FieldTemplateOptions.");
        }
    }
}