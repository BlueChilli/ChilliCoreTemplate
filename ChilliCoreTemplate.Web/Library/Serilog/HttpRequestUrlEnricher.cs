using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Core;
using Serilog.Events;
using System;

namespace ChilliCoreTemplate.Web.Serilog
{
    public class HttpRequestUrlEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpRequestUrlPropertyName = "RequestUrl";
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpRequestUrlEnricher() : this(new HttpContextAccessor())
        {
        }

        public HttpRequestUrlEnricher(IHttpContextAccessor contextAccessor)
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

            string? url = null;

            if (_contextAccessor != null && _contextAccessor.HttpContext != null)
            {
                var context = _contextAccessor.HttpContext;

                url = context.Request.GetDisplayUrl();
            }

            if (url == null) return;

            var property = new LogEventProperty(HttpRequestUrlPropertyName, new ScalarValue(url));
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}