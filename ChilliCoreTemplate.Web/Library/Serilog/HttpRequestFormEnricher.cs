using DryIoc.ImTools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.Serilog
{
    public class HttpRequestFormEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpRequestFormPropertyName = "RequestForm";
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpRequestFormEnricher() : this(new HttpContextAccessor())
        {
        }

        public HttpRequestFormEnricher(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Enrich the log event with the current ASP.NET user name, if User.Identity.IsAuthenticated is true.</summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
                throw new ArgumentNullException("logEvent");

            if (logEvent.Level != LogEventLevel.Error) return;

            List<KeyValuePair<ScalarValue, LogEventPropertyValue>> form = null;

            if (_contextAccessor != null && _contextAccessor.HttpContext != null)
            {
                var context = _contextAccessor.HttpContext;

                if (context.Request.HasFormContentType)
                    form = context.Request.Form.Select(f => new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue(f.Key), new ScalarValue(f.Value))).ToList();
            }

            if (form == null) return;

            var property = new LogEventProperty(HttpRequestFormPropertyName, new DictionaryValue(form));
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}