using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Web.Api;
using ChilliCoreTemplate.Web.Library;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Cloud.Web.MVC.ModelBinding;
using DataTables.AspNet.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using SixLabors.ImageSharp.Web.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Web
{
    public class Startup
    {
        public static ILoggerFactory LoggerFactoryProvider { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddBackgroundTaskQueue();
            services.AddSingleton<CoreHostingEnvironment>();
            services.AddSingleton<StreamedContentPolicySelector>(CreateStreamedContentPolicySelector);

            services.AddSingleton<MvcRouterAccessor>();
            services.AddSingleton<IMvcRouterAccessor>(provider => provider.GetRequiredService<MvcRouterAccessor>());
            services.AddSingleton<ITicketStore, SingletonTicketStore>();
            services.AddScoped<ScopedSessionTicketStore>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddHttpContextAccessor();
            services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});            

            ConfigureAuthentication(services);

            services.AddScoped<IUrlHelper>(CreateUrlHelper);

            ConfigureMvc(services);

            services.RegisterDataTables();

            var registration = new ServiceRegistration(Configuration, LoggerFactoryProvider);
            registration.ConfigureServices(services);

            services.AddSingleton<TaskConfig>();

            ImageSharpExtensions.ConfigureImageSharp(services);

            new SwaggerConfig().ConfigureSwaggerServices(services);
            ConfigureCompression(services);

            services.Configure<ResponseCachingOptions>(options =>
            {
                options.MaximumBodySize = 2 * 1024 * 1024;
            });

            services.AddResponseCaching();

            services.AddMiniProfiler(options =>
            {
                options.UserIdProvider = request => request.HttpContext?.User?.UserData()?.UserId.ToString();
            }).AddEntityFramework();


            services.Configure<RequestLocalizationOptions>(options =>
            {
                var culture = new CultureInfo("en-AU");
                options.SupportedCultures = new List<CultureInfo> { culture };
                options.SupportedUICultures = new List<CultureInfo> { culture };
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture);

                //This removes the default culture reader from the request.
                options.RequestCultureProviders = new List<Microsoft.AspNetCore.Localization.IRequestCultureProvider>();
            });

            return DryIocSetup.Initialise(services);
        }

        private IUrlHelper CreateUrlHelper(IServiceProvider provider)
        {
            var settings = provider.GetRequiredService<ProjectSettings>();
            var actionContext = provider.GetRequiredService<IActionContextAccessor>().ActionContext;
            //There's no action context in background tasks.
            if (actionContext == null)
            {
                var httpContext = new DefaultHttpContext { RequestServices = provider };
                var baseUri = new Uri(settings.BaseUrl);
                httpContext.Request.Scheme = baseUri.Scheme;
                httpContext.Request.Host = HostString.FromUriComponent(baseUri);
                httpContext.Request.PathBase = PathString.FromUriComponent(baseUri);

                var router = provider.GetService<IMvcRouterAccessor>()?.Router;
                var routeData = router == null ? new RouteData() : new RouteData() { Routers = { router } };
                actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
            }

            var factory = provider.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext);
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure<IServiceProvider>((options, provider) =>
                {
                    var config = provider.GetRequiredService<ProjectSettings>();
                    options.SessionStore = provider.GetRequiredService<ITicketStore>();

                    options.LoginPath = new PathString("/EmailAccount/Login");
                    options.AccessDeniedPath = options.LoginPath;
                    options.ExpireTimeSpan = TimeSpan.FromHours(config.SessionLength);
                    options.SlidingExpiration = true;

                    options.Cookie.Name = config.ProjectName.Replace(" ", "");
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.None;

                    options.Events.OnRedirectToLogin = MvcInfrastructureHelper.OnRedirectToLogin;
                    options.Events.OnRedirectToAccessDenied = MvcInfrastructureHelper.OnRedirectToAccessDenied;
                });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie();
        }

        private void ConfigureCompression(IServiceCollection services)
        {
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;

                // options.MimeTypes - allows customization of mimetypes that should be compressed.
                // By default, Jpg and Png will *not* be compressed (they are assumed to be already compressed)
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
        }

        private StreamedContentPolicySelector CreateStreamedContentPolicySelector(IServiceProvider provider)
        {
            return new StreamedContentPolicySelector()
            {
                StreamedResquestRelativePaths = new PathString[] { new PathString("/api/server/testupload") }
            };
        }

        private void ConfigureMvc(IServiceCollection services)
        {
            services.AddOptions<MvcOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    var settings = provider.GetRequiredService<ProjectSettings>();
                    var streamedPolicy = provider.GetRequiredService<StreamedContentPolicySelector>();

                    options.ModelMetadataDetailsProviders.Add(new MetadataAwareProvider());
                    options.Filters.Add(new ApiKeyActionFilter(settings.ApiSettings.ApiKey));
                    options.Filters.Add(new StreamedContentResourceFilter(streamedPolicy));

                    options.EnableEndpointRouting = false;
                });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.AddFlagsEnumModelBinderProvider();
            })
            .AddControllersAsServices()
            .AddNewtonsoftJson()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddCookieTempDataProvider()
            .AddHybridModelBinder()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;

                options.SuppressModelStateInvalidFilter = true;
                //options.SuppressMapClientErrors = true;
            });

            services.AddOptions<MvcNewtonsoftJsonOptions>()
                .Configure<IWebHostEnvironment>((options, env) =>
                {
                    options.AllowInputFormatterExceptionMessages = env.IsDevelopment();
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddOptions<HttpsRedirectionOptions>()
                .Configure<ProjectSettings>((options, settings) =>
                {
                    options.HttpsPort = settings.Hosting.HttpsPort ?? 443;
                });

            //Needed when under ELB
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardLimit = 2;
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("oAuthProvider", typeof(OAuthProviderRouteConstraint));
            });
        }

        private static bool UseDetailedPageErrorHandler(HttpContext context, bool showErrors)
        {
            return showErrors && !context.Request.IsApiRequest();
        }

        readonly static PathString ApiServerPath = new PathString("/api/server");
        readonly static string[] ApiLogExtensionsIgnore = { ".php" };

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var streamedPolicy = app.ApplicationServices.GetRequiredService<StreamedContentPolicySelector>();
            var settings = app.ApplicationServices.GetRequiredService<ProjectSettings>();
            var showErrors = !env.IsProduction();

            if (!env.IsProduction())
            {
                //app.UseMiniProfiler();
            }

            if (settings.Hosting.UnderELB)
            {
                //Needed when under ELB
                app.UseForwardedHeaders();
            }

            app.UseWhen(context => !context.Request.IsApiRequest(), builder =>
            {
                //In practice, only 404 will be handled here, because 500 is already handled by another middleware.
                builder.UseStatusCodePagesWithReExecute("/Error/NotFound");
            });

            app.UseWhen(context => context.Request.IsApiRequest(), builder =>
            {
                builder.Use(async (context, next) =>
                {
                    await next();
                    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                    {
                        await MvcInfrastructureHelper.NotFoundApiHandler(context);
                    }
                });
            });

            app.UseWhen(context => UseDetailedPageErrorHandler(context, showErrors), builder =>
            {
                builder.UseMiddleware<DeveloperExceptionPageMiddleware>();
                builder.UseMiddleware<DatabaseErrorPageMiddleware>();
            });

            app.UseWhen(context => !UseDetailedPageErrorHandler(context, showErrors), builder =>
            {
                builder.UseExceptionHandler(new ExceptionHandlerOptions()
                {
                    ExceptionHandler = (c) => MvcInfrastructureHelper.ExceptionHandler(c, showErrors: showErrors)
                });
            });

            if (settings.Hosting.HttpsOnly)
            {
                if (settings.Hosting.Hsts)
                {
                    app.UseHsts();
                }

                //Allows /api/server/tick to be accessed via HTTP protocol. This is required by the ELB health check
                app.UseWhen(context => !context.Request.Path.Value.StartsWith("/api/server/tick"), builder =>
                {
                    builder.UseHttpsRedirection();
                });
            }

            if (env.IsProduction())
            {
                app.UseCors(policy =>
                {
                    policy.WithOrigins(settings.PublicUrl.Replace("www", "*"), settings.PublicUrl.Replace("www.", ""));
                    policy.SetIsOriginAllowedToAllowWildcardSubdomains();
                    policy.AllowCredentials();
                    policy.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH");
                    policy.WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "ApiKey", "UserKey");
                    policy.SetPreflightMaxAge(TimeSpan.FromDays(1));
                });
            }
            else
            {
                app.UseCors(policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                    policy.SetIsOriginAllowed(o => true);
                    policy.SetPreflightMaxAge(TimeSpan.FromDays(1));
                });
            }

            UseRewriteOptions(app, useIndexHtmlPage: settings.UseIndexHtml);

            app.UseWhen(context => !SVGRequest(context), builder =>
            {
                builder.UseImageSharp();
            });
            app.UseMiddleware<RemoteStorageMiddleware>();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/.well-known")),
                RequestPath = new PathString("/.well-known"),
                ServeUnknownFileTypes = true
            });
            app.UseWhen(context => context.Request.Path.Value.StartsWith("/version_check"), builder =>
            {
                //if version check file was not found as a static file, just return 404.
                builder.Use(async (context, next) =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.Body.FlushAsync();
                    await next();
                });
            });

            app.UseWhen(context => !streamedPolicy.IsStreamedResponse(context), builder =>
            {
                builder.UseResponseCaching();
                builder.UseResponseCompression();
            });

            //app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseWhen(context => context.Request.IsApiRequest(), builder =>
            {
                builder.UseMiddleware<UserKeyMiddleware>();
            });

            //Logs requests after Auth middleware.
            if (settings.ApiSettings.LogApiCalls)
            {
                app.UseWhen(
                    context =>
                        context.Request.IsApiRequest()
                        && !context.Request.Path.StartsWithSegments(ApiServerPath, StringComparison.OrdinalIgnoreCase)
                        && !ApiLogExtensionsIgnore.Any(e => context.Request.Path.Value.EndsWith(e, StringComparison.OrdinalIgnoreCase)),
                    builder =>
                    {
                        builder.UseMiddleware<HttpLogMiddleware>();
                    });
            }

            app.UseRequestLocalization();

            IRouteBuilder capturedRoutes = null;
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Admin",
                    template: "{area:exists}/{controller=Default}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "Company",
                    template: "{area:exists}/{controller=Default}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Public}/{action=Index}/{id?}");

                capturedRoutes = routes;
            });

            var routerAccessor = app.ApplicationServices.GetRequiredService<MvcRouterAccessor>();
            routerAccessor.Router = capturedRoutes.Build(); //this requires MvcOptions.EnableEndpointRouting = false;

            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (settings.UseIndexHtml)
            {
                //This needs to be the last middleware
                //if we reached this point, no other middleware handled the request.

                app.UseMiddleware<IndexPageMiddleware>();
            }

            if (!String.IsNullOrEmpty(settings.GoogleApis.ApiKey))
            {
                GlobalMVCConfiguration.Instance.SetGoogleApisSettings(settings.GoogleApis.ApiKey, settings.GoogleApis.Libraries);
            }

            AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("WebRootPath", env.WebRootPath);
        }

        private bool SVGRequest(HttpContext context)
        {
            var pathValue = context.Request.Path.Value;
            return pathValue.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                   || pathValue.EndsWith(".svgz", StringComparison.OrdinalIgnoreCase);
        }

        private void UseRewriteOptions(IApplicationBuilder app, bool useIndexHtmlPage)
        {
            var options = new RewriteOptions();

            if (useIndexHtmlPage)
            {
                // "indexrequest" is just a placeholder and can be any value not registered in mvc.
                // the last middleware will handle it.
                options.AddRewrite("^$", "/indexrequest", skipRemainingRules: true);
            }
            else
            {
                options.AddRewrite("^$", "/EmailAccount/Login", skipRemainingRules: true);
            }

            //options.AddRedirect("^admin$", "/User/Users");

            app.UseRewriter(options);
        }
    }
}
