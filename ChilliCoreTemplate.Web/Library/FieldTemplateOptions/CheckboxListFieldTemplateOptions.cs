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
    public class CheckboxListFieldTemplateOptions : SelectListFieldTemplateOptionsBase
    {
        public CheckboxListFieldTemplateOptions() : base() { }
        public CheckboxListFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/CheckboxList";
        }

        public CheckBoxAttribute CheckboxAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var baseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;

            if (this.CheckboxAttribute == null)
                this.CheckboxAttribute = member.Member.GetCustomAttribute<CheckBoxAttribute>() ?? new CheckBoxAttribute();

            base.ProcessSelect(baseType, metadata, templateModel);

            return templateModel;
        }
    }
}