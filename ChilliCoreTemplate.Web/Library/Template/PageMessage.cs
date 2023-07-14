using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class PageMessage
    {
        public static string Key() { return $"PageMessage_{Guid.NewGuid()}"; }

        public static string Success(string message, bool isHtml = false, bool isStatic = false)
        {
            return BuidMessage("success", message, isHtml, isStatic);
        }

        public static string Warning(string message, bool isHtml = false, bool isStatic = true)
        {
            return BuidMessage("warning", message, isHtml, isStatic);
        }

        public static string Info(string message, bool isHtml = false, bool isStatic = true)
        {
            return BuidMessage("info", message, isHtml, isStatic);
        }

        public static string Error(string message, bool isHtml = false, bool isStatic = true)
        {
            return BuidMessage("danger", message, isHtml, isStatic);
        }

        private static string BuidMessage(string messageClass, string message, bool isHtml, bool isStatic)
        {
            return new PageMessage
            {
                Message = message,
                MessageClass = messageClass,
                IsHtml = isHtml,
                IsStatic = isStatic
            }.ToJson();
        }

        public string Message { get; set; }
        public string MessageClass { get; set; }
        public bool IsStatic { get; set; }
        public bool IsHtml { get; set; }
    }
}
