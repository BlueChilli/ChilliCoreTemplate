using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ChilliCoreTemplate.Web.TagHelpers
{
    public static class TagHelperExtensions
    {
        public static void AppendAttribute(this TagHelperAttributeList list, string name, object value)
        {
            list.TryGetAttribute(name, out var attribute);
            if (attribute == null)
            {
                list.Add(name, value);
            }
            else
            {
                list.SetAttribute(name, $"{attribute.Value} {value}");
            }
        }
    }
}
