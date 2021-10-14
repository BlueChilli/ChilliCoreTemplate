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
    public abstract class SelectListFieldTemplateOptionsBase : FieldTemplateOptionsBase
    {
        private static readonly SelectListItem[] SingleEmptyItem = { new SelectListItem { Text = "", Value = "" } };

        public SelectListFieldTemplateOptionsBase() : base() { }
        public SelectListFieldTemplateOptionsBase(FieldTemplateOptionsBase other) : base(other) { }

        public IEnumerable<SelectListItem> SelectList { get; set; }

        protected void ProcessSelect(Type baseType, ModelMetadata metadata, IFieldInnerTemplateModel data)
        {
            var listPopulatedByClient = (this.SelectList != null);
            if (listPopulatedByClient)
            {
                if (metadata.ModelType.IsGenericType && metadata.ModelType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    data.HtmlAttributes.Add("multiple", "multiple");
                    var selectedValues = new HashSet<string>((data.Value as IEnumerable)?.Cast<object>().Select(v => v?.ToString())
                                                                ?? Enumerable.Empty<string>());
                    if (data.Value is string) selectedValues = new HashSet<string>((data.Value as string).Split(','));

                    foreach (var item in this.SelectList)
                    {
                        if (selectedValues.Contains(item.Value))
                            item.Selected = true;
                    }
                }
                else
                {
                    var selectedValue = data.Value?.ToString();
                    foreach (var item in this.SelectList)
                    {
                        if (item.Value == selectedValue)
                            item.Selected = true;
                    }
                }
            }
            else if (baseType == typeof(Enum))
            {
                Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
                var values = EnumHelper.GetValues(enumType).Cast<object>();
                var modelValues = data.Value == null ? new string[0] : data.Value.ToString().Split(',');
                for (var i = 0; i < modelValues.Count(); i++) modelValues[i] = modelValues[i].Trim();

                this.SelectList =
                   (from v in values
                    select new SelectListItem
                    {
                        Text = EnumHelper.GetDescription(v),
                        Value = v.ToString(),
                        Selected = modelValues.Contains(v.ToString())
                    }).ToList();
                var flags = enumType.GetCustomAttribute<FlagsAttribute>();
                if (flags != null && !data.HtmlAttributes.ContainsKey("multiple"))
                    data.HtmlAttributes.Add("multiple", "multiple");
            }

            // When SelectList is populated by the client, only resolves EmptyItemAttribute if the attribute is explicitly declared.
            this.SelectList = listPopulatedByClient ?
                                EmptyItemAttribute.Resolve(metadata, this.SelectList)
                                : EmptyItemAttribute.Resolve(metadata, this.SelectList, SingleEmptyItem);

            this.SelectList = RemoveItemAttribute.Resolve(metadata, this.SelectList);
        }
    }
}