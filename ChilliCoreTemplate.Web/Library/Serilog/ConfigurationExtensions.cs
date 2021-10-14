using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChilliCoreTemplate.Web.Serilog
{
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Enrich log events with the UserId property when available in the HttpContext.
        /// Based from: https://raw.githubusercontent.com/serilog-web/classic/d32c124dc9ab917647489f5699bc94745c2252bb/src/SerilogWeb.Classic/SerilogWebClassicLoggerConfigurationExtensions.cs
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithUserId(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With(new UserIdEnricher());
        }

        public static LoggerConfiguration WithExceptionMessage(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With(new ExceptioMessageEnricher());
        }

    }
}