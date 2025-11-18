using Microsoft.AspNetCore.Http;
using System;

namespace ChilliCoreTemplate.Web.Api;

/// <summary>
/// Marks an endpoint as handling large streamed file uploads so middleware can adjust behavior
/// (e.g. disable buffering, modify logging, extend timeouts).
/// </summary>

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StreamedRequestAttribute : Attribute
{
    internal static bool IsSet(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(StreamedDetectionMiddleware.StreamedRequestFlagKey, out var srFlag) && srFlag is bool sr && sr;
    }
}

/// <summary>
/// Marks an endpoint as handling large streamed file downloads so middleware can adjust behavior
/// (e.g. disable buffering, modify logging, extend timeouts).
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StreamedResponseAttribute : Attribute
{
    internal static bool IsSet(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(StreamedDetectionMiddleware.StreamedResponseFlagKey, out var sResFlag) && sResFlag is bool sres && sres;
    }
}