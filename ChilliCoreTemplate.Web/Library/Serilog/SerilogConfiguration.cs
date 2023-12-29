using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Serilog
{
    public static class SerilogConfiguration
    {
        public static LoggerConfiguration Configure(IConfigurationRoot config)
        {
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs\\log.txt");
            EnsureDirectory(logFilePath);
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Filter.ByExcluding("RequestPath like '%/swagger/%'")   //Bug with imagesharp config try to process virtual image => The image '"https://xxxxx/swagger/favicon-32x32.png"' could not be resolved
                .Filter.ByExcluding("SourceContext = 'Microsoft.AspNetCore.Server.IIS.Core.IISHttpServer'") //Bug in Microsoft.AspNetCore.Server.IIS.Core.IISHttpContext.GetOriginalPath() during warm up
                .Filter.ByExcluding(x => x.Exception?.Message == "Could not obtain database time information") //Common nuisance error when connecting with low spec azure databases (Scheduled tasks)
                .Filter.ByExcluding(x => x.Exception?.Message == "The antiforgery token could not be decrypted.") //Encryption changes after a release
                .Enrich.FromLogContext()
                .Enrich.WithHttpRequestUrl()
                .Enrich.WithHttpRequestForm()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers().WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
                .Enrich.WithExceptionMessage()
                .Enrich.WithUserId()
                //.Enrich.With<CustomExceptionDataEnricher>()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                //.Enrich.WithHttpRequestRawUrl()
                //.Enrich.WithHttpRequestType()
                //.Enrich.WithHttpRequestUrl()
                //.Enrich.WithHttpRequestUrlReferrer()
                //.Enrich.WithHttpRequestUserAgent()
                //.Enrich.WithMvcActionName()
                //.Enrich.WithMvcControllerName()
                //.Enrich.WithWebApiRouteData()
                //.Enrich.WithWebApiControllerName()
                //.Enrich.WithWebApiActionName()
                .WriteTo.File(logFilePath, fileSizeLimitBytes: 1024 * 1024, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(10),
                    rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 15);

            var options = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
                    {
                        new SqlColumn { ColumnName = UserIdEnricher.UserIdPropertyName, DataType = System.Data.SqlDbType.Int, AllowNull = true },
                        new SqlColumn { ColumnName = ExceptioMessageEnricher.ExceptionMessagePropertyName, DataType = System.Data.SqlDbType.NVarChar, AllowNull = true }
                    }
            };
            options.Store.Remove(StandardColumn.Properties);
            options.Store.Add(StandardColumn.LogEvent);
            options.TimeStamp.ConvertToUtc = true;
            loggerConfig
                .WriteTo.MSSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    sinkOptions: new SinkOptions { TableName = "ErrorLogs" },
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    columnOptions: options
                );

            return loggerConfig;
        }

        private static void EnsureDirectory(string logFilePath)
        {
            var dirName = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

    }
}
