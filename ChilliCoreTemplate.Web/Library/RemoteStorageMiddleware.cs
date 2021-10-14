using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class RemoteStorageMiddleware
    {
        RequestDelegate _next;
        RemoteStorageMiddlewareOptions _options;
        PathString _pathPrefix;
        IRemoteStorage _remoteStorage;
        ILogger _logger;

        public RemoteStorageMiddleware(RequestDelegate next, IOptions<RemoteStorageMiddlewareOptions> optionsAcessor, IRemoteStorage remoteStorage, ILoggerFactory loggerFactory)
        {
            _next = next;
            _options = optionsAcessor.Value;

            if (String.IsNullOrEmpty(_options.UrlPrefix))
            {
                throw new ArgumentException("UrlPrefix value is invalid.");
            }

            _pathPrefix = new PathString(_options.UrlPrefix.TrimStart('~'));
            _remoteStorage = remoteStorage;
            _logger = loggerFactory?.CreateLogger<RemoteStorageMiddleware>();
        }

        protected bool IsAMatch(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_pathPrefix) 
                    && _options.AllowedExtensions.Any(ext => context.Request.Path.Value.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private string GetRelativeFileName(HttpContext context)
        {
            return context.Request.Path.Value.Substring(_pathPrefix.Value.Length).TrimStart('/');
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (IsAMatch(httpContext) && (await InvokeInternal(httpContext).IgnoreContext()))
                return;

            await _next.Invoke(httpContext).IgnoreContext();
        }

        public async Task<bool> InvokeInternal(HttpContext httpContext)
        {
            var fileName = GetRelativeFileName(httpContext);
            if (String.IsNullOrEmpty(fileName))
                return false;

            FileStorageResponse remoteResponse = null;
            try
            {
                remoteResponse = await _remoteStorage.GetContentAsync(fileName, httpContext.RequestAborted)
                                        .IgnoreContext();
            }
            catch
            {
                /* no op */
                return false;
            }

            if (remoteResponse == null)
                return false;

            using (remoteResponse.Stream)
            {
                httpContext.Response.ContentLength = remoteResponse.ContentLength;
                httpContext.Response.ContentType = String.IsNullOrEmpty(remoteResponse.ContentType) ? "application/octet-stream" : remoteResponse.ContentType;

                if (!String.IsNullOrEmpty(remoteResponse.CacheControl))
                {
                    httpContext.Response.Headers["Cache-Control"] = remoteResponse.CacheControl;
                }
                else if (!String.IsNullOrEmpty(_options.DefaultCacheControl))
                {
                    httpContext.Response.Headers["Cache-Control"] = _options.DefaultCacheControl;
                }

                if (!String.IsNullOrEmpty(remoteResponse.ContentEncoding))
                    httpContext.Response.Headers["Content-Encoding"] = remoteResponse.ContentEncoding;

                if (!String.IsNullOrEmpty(remoteResponse.ContentDisposition))
                    httpContext.Response.Headers["Content-Disposition"] = remoteResponse.ContentDisposition;

                var headers = httpContext.Response.GetTypedHeaders();
                if (!remoteResponse.LastModifiedUtc.Equals(DateTime.MinValue))
                    headers.LastModified = new DateTimeOffset(remoteResponse.LastModifiedUtc);

                var buffersize = (int)Math.Min(remoteResponse.ContentLength, 32 * 1024);
                if (buffersize > 0)
                {
                    await remoteResponse.Stream.CopyToAsync(httpContext.Response.Body, buffersize, httpContext.RequestAborted)
                            .IgnoreContext();

                    await httpContext.Response.Body.FlushAsync(httpContext.RequestAborted)
                            .IgnoreContext();
                }
            }

            return true;
        }
    }
}
