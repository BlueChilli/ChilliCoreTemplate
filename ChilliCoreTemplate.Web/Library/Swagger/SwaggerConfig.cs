using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Web.Library.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class SwaggerConfig
    {
        public void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddOptions<SwaggerOptions>()
                .Configure<ProjectSettings>((c, settings) =>
                {
                    c.RouteTemplate = "swagger/docs/{documentName}";
                });

            services.AddOptions<SwaggerGenOptions>()
                .Configure<ProjectSettings>((c, settings) =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = typeof(Startup).Assembly.GetName().Name, Version = "v1" });

                    c.DescribeAllParametersInCamelCase();

                    var webAssembly = Assembly.GetExecutingAssembly();
                    var baseDirectory = GetAssemblyDirectory(webAssembly);
                    var modelAssembly = typeof(AccountCommon).Assembly;

                    c.IncludeXmlComments(Path.Combine(baseDirectory, $"{baseDirectory}/{webAssembly.GetName().Name}.xml"));
                    c.IncludeXmlComments(Path.Combine(baseDirectory, $"{baseDirectory}/{modelAssembly.GetName().Name}.xml"));

                    c.AddSecurityDefinition("apiKeyHeader", new OpenApiSecurityScheme { Name = "ApiKey", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Description = "Pubically known key defined per environment" });
                    c.AddSecurityDefinition("userKeyHeader", new OpenApiSecurityScheme { Name = "UserKey", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Description = "User session key for cookieless authentication (private)" });

                    c.OperationFilter<SwaggerOperationFilter>();
                    c.SchemaFilter<AddSwaggerFoolProofSchemaFilter>();
                });

            services.AddOptions<SwaggerUIOptions>()
                .Configure<ProjectSettings>((c, settings) =>
                {
                    c.SwaggerEndpoint("docs/v1", typeof(Startup).Assembly.GetName().Name);
                });

            services.AddSwaggerGen(o =>
            {
                o.OperationFilter<HybridOperationFilter>();
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        private static string GetAssemblyDirectory(Assembly assembly)
        {
            string filePath = new Uri(assembly.CodeBase).LocalPath;
            return Path.GetDirectoryName(filePath);            
        }
    }
}
