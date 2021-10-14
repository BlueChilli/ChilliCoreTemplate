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
    public class ClockPickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public ClockPickerFieldTemplateOptions() : base() { }
        public ClockPickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return "FieldTemplates/ClockPicker";
        }
    }
}