using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class PageMessage
    {
        public static string Key() { return $"PageMessage_{Guid.NewGuid().ToString()}"; }

        public static string Success(string message, bool isHtml = false)
        {
            return new PageMessage
            {
                Message = message,
                MessageClass = "success",
                IsHtml = isHtml
            }.ToJson();
        }

        public static string Warning(string message, bool isHtml = false)
        {
            return new PageMessage
            {
                Message = message,
                MessageClass = "warning",
                IsHtml = isHtml,
                IsStatic = true
            }.ToJson();
        }

        public static string Info(string message, bool isHtml = false)
        {
            return new PageMessage
            {
                Message = message,
                MessageClass = "info",
                IsHtml = isHtml,
                IsStatic = true
            }.ToJson();
        }

        public string Message { get; set; }
        public string MessageClass { get; set; }
        public bool IsStatic { get; set; }
        public bool IsHtml { get; set; }
    }
}
