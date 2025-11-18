using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api;

/// <summary>
/// Matches attribute-routed MVC actions and sets flags in HttpContext.Items for streamed request/response.
/// Needed because endpoint routing is disabled so HttpContext.GetEndpoint() is null.
/// </summary>
public class StreamedDetectionMiddleware
{
    internal const string StreamedRequestFlagKey = "__StreamedRequest";
    internal const string StreamedResponseFlagKey = "__StreamedResponse";

    private readonly RequestDelegate _next;
    private readonly IActionDescriptorCollectionProvider _actions;

    private readonly ConcurrentDictionary<string, (TemplateMatcher matcher, string[] httpMethods, bool streamedReq, bool streamedRes)> _cache
        = new ConcurrentDictionary<string, (TemplateMatcher, string[], bool, bool)>();

    public StreamedDetectionMiddleware(RequestDelegate next, IActionDescriptorCollectionProvider actions)
    {
        _next = next;
        _actions = actions;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        foreach (var ad in _actions.ActionDescriptors.Items.OfType<ControllerActionDescriptor>())
        {
            var template = ad.AttributeRouteInfo?.Template;
            if (string.IsNullOrEmpty(template)) continue;

            var entry = _cache.GetOrAdd(ad.Id, _ =>
            {
                var parsed = TemplateParser.Parse(template);
                var matcher = new TemplateMatcher(parsed, defaults: new Microsoft.AspNetCore.Routing.RouteValueDictionary());

                string[] methods = ad.ActionConstraints?
                    .OfType<Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint>()
                    .FirstOrDefault()?.HttpMethods?.ToArray() ?? Array.Empty<string>();

                var streamedReq = ad.MethodInfo.IsDefined(typeof(StreamedRequestAttribute), inherit: false);
                var streamedRes = ad.MethodInfo.IsDefined(typeof(StreamedResponseAttribute), inherit: false);
                return (matcher, methods, streamedReq, streamedRes);
            });

            var values = new Microsoft.AspNetCore.Routing.RouteValueDictionary();
            if (entry.matcher.TryMatch(path, values))
            {
                if (entry.httpMethods.Length == 0 ||
                    entry.httpMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
                {
                    if (entry.streamedReq)
                        context.Items[StreamedRequestFlagKey] = true;
                    if (entry.streamedRes)
                        context.Items[StreamedResponseFlagKey] = true;
                    break;
                }
            }
        }

        await _next(context);
    }
}