using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public class HttpLogMiddleware : DelegatingHandler
    {
        private RequestDelegate _next;

        public HttpLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, StreamedContentPolicySelector streamedPolicy, IBackgroundTaskQueue taskQueue)
        {
            var request = httpContext.Request;

            var extensionsToIgnore = new List<string>() { ".php" };
            if (request.Path.HasValue && extensionsToIgnore.Any(e => request.Path.Value.EndsWith(e, StringComparison.OrdinalIgnoreCase))) return;

            var apiLogEntry = CreateApiLogEntryWithRequestData(httpContext);

            var wasException = false;

            try
            {
                //skip content for streamed requests
                if (streamedPolicy.IsStreamedRequest(httpContext))
                {
                    apiLogEntry.RequestContentBody = "<< streamed content not available >>";
                }
                else
                {
                    request.EnableBuffering();
                    apiLogEntry.RequestContentBody = await ReadAsStringAsync(request.Body, Encoding.UTF8, 4 * 1024, 30 * 1024);
                    if (String.IsNullOrEmpty(apiLogEntry.RequestContentBody)) apiLogEntry.RequestContentBody = null;
                    else if (apiLogEntry.RequestContentType != null && apiLogEntry.RequestContentType.Contains("application/json") && apiLogEntry.RequestContentBody.Length < 30 * 1024)
                    {
                        var blacklist = new string[] { "password", "pin" };
                        apiLogEntry.RequestContentBody = apiLogEntry.RequestContentBody.MaskFields(blacklist, "*****");
                    }
                    request.Body.Position = 0;
                }

                if (streamedPolicy.IsStreamedResponse(httpContext))
                {
                    apiLogEntry.ResponseContentBody = "<< streamed content not available >>";
                    await _next(httpContext);
                }
                else
                {
                    apiLogEntry.ResponseContentBody = await ExecutePipelineAndCaptureResponseAsync(httpContext, _next);
                }
            }
            catch (Exception)
            {
                wasException = true;
                throw;
            }
            finally
            {
                var response = httpContext.Response;

                // Update the API log entry with response info
                apiLogEntry.ResponseStatusCode = wasException ? 500 : response.StatusCode;
                apiLogEntry.ResponseTimestamp = DateTime.UtcNow;
                apiLogEntry.ResponseContentType = response.ContentType;
                apiLogEntry.ResponseHeaders = SerializeHeaders(response.Headers);

                taskQueue.QueueBackgroundWorkItem((ct) => SaveEntry(apiLogEntry));
            }
        }

        private async static Task<string> ExecutePipelineAndCaptureResponseAsync(HttpContext httpContext, RequestDelegate next)
        {
            var response = httpContext.Response;
            var originalResponseBody = response.Body;
            var logResponseBody = new MemoryStream(1024);
            response.Body = logResponseBody;

            await next(httpContext);

            logResponseBody.Position = 0;
            var responseContentBody = await ReadAsStringAsync(logResponseBody, Encoding.UTF8, 4 * 1024, maxReadSize: 32 * 1024);

            logResponseBody.Position = 0;
            response.Body = originalResponseBody;
            var buffersize = (int)Math.Min(32 * 1024, logResponseBody.Length);
            if (buffersize > 0)
            {
                await logResponseBody.CopyToAsync(response.Body, buffersize);
            }

            return responseContentBody;
        }

        private static async Task<string> ReadAsStringAsync(Stream stream, Encoding encoding, int bufferSize, int maxReadSize)
        {
            char[] buffer = new char[bufferSize / 2];

            using (var stWriter = new StringWriter(new StringBuilder(buffer.Length)))
            {
                using (var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize, leaveOpen: true))
                {
                    int read = 0;
                    int total = 0;
                    while ((read = await reader.ReadBlockAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        total += read;
                        if (total <= maxReadSize)
                        {
                            stWriter.Write(buffer, 0, read);
                        }
                    }
                }

                return stWriter.ToString();
            }
        }

        private static async Task SaveEntry(ApiLogEntry apiLogEntry)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<ApiServices>();
                await service.Api_Log_SaveAsync(apiLogEntry);
            }
        }

        private ApiLogEntry CreateApiLogEntryWithRequestData(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var httpConnectionFeature = httpContext.Features.Get<IHttpConnectionFeature>();

            return new ApiLogEntry
            {
                RequestTimestamp = DateTime.UtcNow,
                User = httpContext.User?.Identity?.Name,
                Machine = Environment.MachineName,
                RequestContentType = request.ContentType,
                RequestIpAddress = httpConnectionFeature?.RemoteIpAddress?.ToString(),
                RequestMethod = request.Method,
                RequestHeaders = SerializeHeaders(request.Headers),
                RequestUri = UriHelper.GetDisplayUrl(request)
            };
        }

        private string SerializeHeaders(IHeaderDictionary headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                var header = String.Empty;
                foreach (var value in item.Value)
                {
                    header += value + " ";
                }

                // Trim the trailing space and add item to the dictionary
                header = header.TrimEnd(" ".ToCharArray());

                if (item.Key == "Cookie")
                {
                    header = String.Concat(header.Split(';').SelectMany(x => x.Take(20)));
                }

                dict.Add(item.Key, header);
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }
    }
}