using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public class IndexPageMiddleware
    {
        ILogger _logger;
        IWebHostEnvironment _env;
        Task<ReadOnlyMemory<byte>?> _contentProviderTask;
        RequestDelegate _next;

        public IndexPageMiddleware(RequestDelegate next, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<IndexPageMiddleware>();
            _env = env;
            _contentProviderTask = Task.Run(GetIndexPageContent);
        }

        private async Task<ReadOnlyMemory<byte>?> GetIndexPageContent()
        {
            ReadOnlyMemory<byte>? indexContent = null;
            try
            {
                var file = Path.Combine(_env.WebRootPath, "index.html");
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    var textContent = await reader.ReadToEndAsync();
                    indexContent = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(textContent));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error while reading index page content.");
            }

            return indexContent;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Response.HasStarted || httpContext.Request.IsApiRequest() || !httpContext.Request.Method.Same("GET"))
            {
                await _next.Invoke(httpContext);
                return;
            }

            var accept = httpContext.Request.GetTypedHeaders().Accept;
            var hasTextMedia = accept?.Any(a => a.MediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) ?? false;
            var hasImageVideoMedia = accept?.Any(a => a.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || a.MediaType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasTextMedia && hasImageVideoMedia)
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.Body.FlushAsync();
            }
            else
            {
                var indexContent = await _contentProviderTask;
                if (indexContent == null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                httpContext.Response.ContentType = "text/html";

                await httpContext.Response.Body.WriteAsync(indexContent.Value, httpContext.RequestAborted);
            }
        }
    }
}