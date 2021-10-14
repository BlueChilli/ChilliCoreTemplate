using System;
using System.Web;
using Serilog.Core;
using Serilog.Events;

namespace ChilliCoreTemplate.Web.Serilog
{
    /// <summary>
    /// Enrich log events with an exception description when available in the LogEvent.
    /// Based from https://raw.githubusercontent.com/serilog-web/classic/master/src/SerilogWeb.Classic/Classic/Enrichers/UserNameEnricher.cs
    /// </summary>
    public class ExceptioMessageEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string ExceptionMessagePropertyName = "ExceptionMessage";

        /// <summary>
        /// Enrich the log event with the current ASP.NET user name, if User.Identity.IsAuthenticated is true.</summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
                throw new ArgumentNullException("logEvent");

            string exceptionMessage = null;

            if (logEvent.Exception != null)
            {
                exceptionMessage = $"{logEvent.Exception.GetType().Name}: {logEvent.Exception.Message}";
            }

            if (exceptionMessage == null)
                return;

            var exceptionMessageProperty = new LogEventProperty(ExceptionMessagePropertyName, new ScalarValue(exceptionMessage));
            logEvent.AddPropertyIfAbsent(exceptionMessageProperty);
        }
    }
}