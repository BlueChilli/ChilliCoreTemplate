using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;

namespace ChilliCoreTemplate.Web.Serilog
{
    /// <summary>
    /// Enrich log events with the UserId property when available in the HttpContext.
    /// Based from https://github.com/serilog/serilog-aspnetcore/issues/157
    /// </summary>
    public class UserIdEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string UserIdPropertyName = "UserId";
        private readonly IHttpContextAccessor _contextAccessor;

        public UserIdEnricher() : this(new HttpContextAccessor())
        {
        }

        public UserIdEnricher(IHttpContextAccessor contextAccessor)
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

            int? userId = null;

            if (_contextAccessor != null && _contextAccessor.HttpContext != null)
            {
                var context = _contextAccessor.HttpContext;

                if (context.User != null)
                {
                    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
                    {
                        userId = context.User.UserData()?.UserId;
                    }
                }
            }

            if (userId == null)
                return;

            var userIdProperty = new LogEventProperty(UserIdPropertyName, new ScalarValue(userId));
            logEvent.AddPropertyIfAbsent(userIdProperty);
        }
    }
}