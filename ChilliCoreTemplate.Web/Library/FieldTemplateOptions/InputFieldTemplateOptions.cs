using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Web
{
    public class InputFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public InputFieldTemplateOptions() : base() { }
        public InputFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }

        public override string GetViewPath()
        {
            return "FieldTemplates/Input";
        }

        public static void ResolveInputAttributes(IFieldInnerTemplateModel templateModel, string inputType)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;

            PlaceholderAttribute.Resolve(metadata, templateModel.HtmlAttributes);
            HtmlHelperExtensions.ResolveStringLength(member, templateModel.HtmlAttributes);
            templateModel.HtmlAttributes.AddOrSkipIfExists("type", inputType);
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var modelValue = templateModel.InnerMetadata.ModelValue;

            var inputType = "text";

            switch (templateModel.InnerMetadata.MemberUnderlyingType.Name)
            {
                case "Boolean":
                    {
                        inputType = metadata.AdditionalValues.ContainsKey("Radio") ? "radio" : "checkbox";
                        if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(templateModel.Value))
                        {
                            templateModel.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
                        }
                        templateModel.Value = Boolean.TrueString;
                        break;
                    }
                case "Int32":
                    {
                        if (metadata.DataTypeName == null || metadata.DataTypeName == DataType.Currency.ToString())
                        {
                            inputType = "number";
                        }
                        break;
                    }
                case "Decimal":
                    {
                        if (metadata.DataTypeName == null || metadata.DataTypeName == DataType.Currency.ToString())
                        {
                            inputType = "number";
                            templateModel.HtmlAttributes.Add("Step", "any");
                        }
                        break;
                    }
                default:
                    {
                        switch (metadata.DataTypeName)
                        {
                            case "Password":
                                {
                                    inputType = "password";
                                    templateModel.Value = "";
                                    break;
                                }
                            case "EmailAddress":
                                {
                                    inputType = "Email";
                                    break;
                                }
                            case "Url":
                                {
                                    inputType = "Url";
                                    break;
                                }
                        }
                        break;
                    }
            }

            switch (metadata.DataTypeName)
            {
                case "Currency":
                    this.PreAddOn = MvcHtmlStringCompatibility.Create("<span class=\"input-group-addon\">$</span>");
                    break;

            }

            ResolveInputAttributes(templateModel, inputType);

            return templateModel;
        }
    }
}