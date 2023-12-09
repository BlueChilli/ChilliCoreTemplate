using AutoMapper;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Web.Serilog;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = SetupConfigurationBuilder(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()), args: args).Build();

            var loggerConfig = SerilogConfiguration.Configure(config);
            Log.Logger = loggerConfig.CreateLogger();

            Startup.LoggerFactoryProvider = new LoggerFactory(new[] { new SerilogLoggerProvider(Log.Logger) });

            try
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 256;
                ThreadPool.SetMinThreads(100, 200);

                var host = BuildWebHost(args);
                var coreHostingEnvironment = host.Services.GetRequiredService<CoreHostingEnvironment>();
                var taskConfig = host.Services.GetRequiredService<TaskConfig>();

                CoreDbProviderFactories.RegisterFactory("System.Data.SqlClient", () => System.Data.SqlClient.SqlClientFactory.Instance);
                GlobalConfiguration.Instance.SetHostingEnvironment(coreHostingEnvironment);
                GlobalConfiguration.Instance.SetMimeMapping(new WebMimeMapping());
                GlobalConfiguration.Instance.SetLogger(Log.Logger);

                TemplateOptions.DefaultFieldTemplateLayout = () => FieldTemplateLayouts.StandardField;
                TemplateOptions.DefaultFieldTemplateOptions = () => new FieldTemplateOptions();
                ServiceCallerOptions.ViewNamingConvention = ViewNamingConvention.ControllerPrefix;  //Allow to override this per controller

                LinqMappers.Configure(host.Services);
                DatabaseInitialization.Initialize(host.Services);

                taskConfig.RegisterTasks();
                taskConfig.StartListenner();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();

                System.Console.WriteLine("Host has been shut down.");
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return CreateWebHostBuilder(args)
                    .UseShutdownTimeout(TimeSpan.FromSeconds(30))
                    .UseStartup<Startup>()
                    .UseSerilog()
                    .Build();
        }

        private static IConfigurationBuilder SetupConfigurationBuilder(IConfigurationBuilder config, string[] args)
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{AppSettingsEnvironment.BuildConfigurationName}.json", optional: true, reloadOnChange: true);

            config.AddEnvironmentVariables();

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            return config;
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = new WebHostBuilder();

            if (string.IsNullOrEmpty(builder.GetSetting(WebHostDefaults.ContentRootKey)))
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
            }
            if (args != null)
            {
                builder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
            }

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                env.EnvironmentName = AppSettingsEnvironment.EnvironmentName;

                SetupConfigurationBuilder(config, args);

                if (env.IsDevelopment())
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                }
            })
            .UseKestrel((builderContext, options) =>
            {
                options.AddServerHeader = false;
                options.Configure(builderContext.Configuration.GetSection("Kestrel"));
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                //logging.AddConsole();
                //logging.AddDebug();
                //logging.AddEventSourceLogger();
            })
            .UseIIS()
            .UseIISIntegration()
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            });

            return builder;
        }
    }
}
