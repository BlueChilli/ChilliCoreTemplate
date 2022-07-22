using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.Configuration;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using LazyCache;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ChilliSource.Cloud.ImageSharp;
using ChilliCoreTemplate.Service.Sms;
using Microsoft.AspNetCore.DataProtection;

namespace ChilliCoreTemplate.Service
{
    public class ServiceRegistration
    {
        private static readonly Lazy<List<Type>> ServiceTypesCache = new Lazy<List<Type>>(() => GetServiceTypes().ToList());

        public static IEnumerable<Type> GetServiceTypes()
        {
            var types = typeof(ChilliCoreTemplate.Service.IService).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsAbstract && typeof(ChilliCoreTemplate.Service.IService).IsAssignableFrom(type))
                    yield return type;
            }
        }

        public ServiceRegistration(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        private IFileStorage CreateFileStorage(IServiceProvider provider)
        {
            return provider.GetRequiredService<FileStorageHelper>().CreateFileStorage();
        }

        public void ConfigureDbOptions(DbContextOptionsBuilder builder)
        {
            builder.UseLoggerFactory(LoggerFactory);

            builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                    .UseNetTopologySuite());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(ConfigureDbOptions);
            services.AddDataProtection().PersistKeysToDbContext<DataContext>();

            services.AddSingleton<ProjectSettings>(provider => new ProjectSettings(provider.GetRequiredService<IConfiguration>()));
            services.AddSingleton<Models.Api.PushNotificationSettings>(provider => new Models.Api.PushNotificationSettings(provider.GetRequiredService<IConfiguration>()));
            services.AddSingleton<FileStoragePath>();
            services.AddSingleton<FileStorageHelper>();
            services.AddSingleton<IRemoteStorage>(provider => provider.GetRequiredService<FileStorageHelper>().CreateRemoteStorage());

            services.AddSingleton<ILockManager>(CreateLockManager);
            services.AddSingleton<ITaskManager>(CreateTaskManager);
            services.AddSingleton<IFileStorage>(CreateFileStorage);

            services.AddOptions<RemoteStorageMiddlewareOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    var storageHelper = provider.GetRequiredService<FileStorageHelper>();
                    options.UrlPrefix = storageHelper.GetImagePrefix();
                    options.DefaultCacheControl = "private";

                    options.AllowedExtensions.Clear();
                    options.AllowedExtensions.Add(".svg");
                    options.AllowedExtensions.Add(".svgz");
                });

            services.AddOptions<CloudStorageImageProviderOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    var storageHelper = provider.GetRequiredService<FileStorageHelper>();
                    options.UrlPrefix = storageHelper.GetImagePrefix();
                });

            services.AddLazyCache(sp => new CachingService(CachingService.DefaultCacheProvider));

            services.AddScoped<UserKeyHelper>();
            services.AddScoped<UserSessionService>();
            services.AddScoped<IEmailQueue, AsyncDispatchEmailQueue>();
            services.AddScoped<IEmailSender>(CreateEmailSender);
            services.AddScoped<ISmsQueue, AsyncDispatchSmsQueue>();
            services.AddSingleton<SmsServiceFactory>();
            services.AddScoped<ISmsService>(provider => provider.GetRequiredService<SmsServiceFactory>().CreateService());

            services.AddOptions<TemplateViewRendererOptions>()
                .Configure<ProjectSettings>((options, settings) =>
                {
                    options.BaseUri = new Uri(settings.BaseUrl);
                });
            services.AddScoped<ITemplateViewRenderer, TemplateViewRenderer>();

            ServiceTypesCache.Value.ForEach(type => services.AddScoped(type));
        }

        private IEmailSender CreateEmailSender(IServiceProvider provider)
        {
            return new EmailSender(
                provider.GetRequiredService<ProjectSettings>(),
                provider.GetRequiredService<IFileStorage>(),
                (config) => new EmailClient(config.Host, config.Port, config.UserName,
                    config.Password, config.EnableSsl),
                provider.GetService<ILogger>());
        }

        private ILockManager CreateLockManager(IServiceProvider provider)
        {
            return LockManagerFactory.Create(() =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                ConfigureDbOptions(optionsBuilder);

                return new DataContext(optionsBuilder.Options);
            });
        }

        private ITaskManager CreateTaskManager(IServiceProvider provider)
        {
            return TaskManagerFactory.Create(() =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                ConfigureDbOptions(optionsBuilder);

                return new DataContext(optionsBuilder.Options);
            }, new TaskManagerOptions() { MainLoopWait = 5000 });
        }
    }
}
