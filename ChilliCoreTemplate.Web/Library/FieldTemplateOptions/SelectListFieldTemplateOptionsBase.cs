using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public abstract class SelectListFieldTemplateOptionsBase : FieldTemplateOptionsBase
    {
        private static readonly SelectListItem[] SingleEmptyItem = { new SelectListItem { Text = "", Value = "" } };

        public SelectListFieldTemplateOptionsBase() : base() { }
        public SelectListFieldTemplateOptionsBase(FieldTemplateOptionsBase other) : base(other) { }

        public IEnumerable<SelectListItem> SelectList { get; set; }

        protected void ProcessSelect(Type baseType, ModelMetadata metadata, IFieldInnerTemplateModel data, bool isRequired = false)
        {
            var listPopulatedByClient = (this.SelectList != null);
            if (baseType == typeof(Enum) && !listPopulatedByClient)
            {
                Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
                var values = EnumHelper.GetValues(enumType).Cast<object>();
                var modelValues = data.Value == null ? new string[0] : data.Value.ToString().Split(',');
                for (var i = 0; i < modelValues.Count(); i++) modelValues[i] = modelValues[i].Trim();

                var groupsByName = new Dictionary<string, SelectListGroup>(StringComparer.OrdinalIgnoreCase);

                this.SelectList =
                   (from v in values
                    let member = enumType.GetMember(v.ToString()).FirstOrDefault()
                    let groupAttr = member?.GetCustomAttribute<GroupAttribute>()
                    let groupName = groupAttr?.Name
                    let @group = groupName == null ? null
                                : groupsByName.TryGetValue(groupName, out var existing)
                                    ? existing
                                    : (groupsByName[groupName] = new SelectListGroup { Name = groupName })
                    select new SelectListItem
                    {
                        Text = EnumHelper.GetDescription(v),
                        Value = v.ToString(),
                        Selected = modelValues.Contains(v.ToString()),
                        Group = @group
                    }).ToList();

                var flags = enumType.GetCustomAttribute<FlagsAttribute>();
                if (flags != null && !data.HtmlAttributes.ContainsKey("multiple"))
                    data.HtmlAttributes.Add("multiple", "multiple");
            }
            else
            {
                if (metadata.ModelType.IsGenericType && metadata.ModelType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    data.HtmlAttributes.Add("multiple", "multiple");
                    if (SelectList != null)
                    {
                        var selectedValues = new HashSet<string>((data.Value as IEnumerable)?.Cast<object>().Select(v => v?.ToString())
                                                                    ?? Enumerable.Empty<string>());
                        if (data.Value is string) selectedValues = new HashSet<string>((data.Value as string).Split(','));

                        foreach (var item in this.SelectList)
                        {
                            if (selectedValues.Contains(item.Value))
                                item.Selected = true;
                        }
                    }
                }
                else if (SelectList != null)
                {
                    var currentValue = data.Value?.ToString();
                    var hasCurrentValue = SelectList.Any(x => x.Value == currentValue);
                    if (hasCurrentValue)
                    {
                        foreach (var item in this.SelectList)
                        {
                            item.Selected = item.Value == currentValue;
                        }
                    }
                }
            }

            if (this is not RadioListFieldTemplateOptions)
            {
                // When SelectList is populated by the client, only resolves EmptyItemAttribute if the attribute is explicitly declared.
                this.SelectList = listPopulatedByClient ?
                                    EmptyItemAttribute.Resolve(metadata, this.SelectList, isRequired)
                                    : EmptyItemAttribute.Resolve(metadata, this.SelectList, SingleEmptyItem, isRequired);
            }

            this.SelectList = RemoveItemAttribute.Resolve(metadata, this.SelectList);
        }
    }
}