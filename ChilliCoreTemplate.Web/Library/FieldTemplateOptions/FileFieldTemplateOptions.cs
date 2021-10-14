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
    public class FileFieldTemplateOptions : FieldTemplateOptionsBase
    {        
        public FileFieldTemplateOptions() : base() { }
        public FileFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public string ButtonText { get; set; } = "Choose";

        public override string GetViewPath()
        {
            return "FieldTemplates/File";
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            HttpPostedFileExtensionsAttribute.Resolve(metadata, templateModel.HtmlAttributes);

            return templateModel;
        }
    }
}