using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ChilliSource.Cloud.Web.MVC;

namespace ChilliCoreTemplate.Web
{
    public class CheckboxFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public CheckboxFieldTemplateOptions() : base() { }
        public CheckboxFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/Checkbox";
        }

        public CheckBoxAttribute CheckBoxAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var member = templateModel.InnerMetadata.MemberExpression;

            if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(templateModel.Value))
            {
                templateModel.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
            }
            templateModel.HtmlAttributes.AddOrSkipIfExists("type", "checkbox");

            if (this.CheckBoxAttribute == null)
                this.CheckBoxAttribute = member.Member.GetCustomAttribute<CheckBoxAttribute>();

            return templateModel;
        }
    }
}