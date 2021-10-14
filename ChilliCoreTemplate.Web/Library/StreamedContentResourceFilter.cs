using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class StreamedContentResourceFilter : IResourceFilter
    {
        StreamedContentPolicySelector _contentSelector;
        public StreamedContentResourceFilter(StreamedContentPolicySelector contentSelector)
        {
            if (contentSelector == null)
                throw new ArgumentNullException(nameof(contentSelector));

            _contentSelector = contentSelector;
        }

        private IEnumerable<IValueProviderFactory> FilterFactories(IEnumerable<IValueProviderFactory> factories)
        {
            foreach (var factory in factories)
            {
                // RouteValueProviderFactory and QueryStringValueProviderFactory don't read the content body 
                // and can be used on requests that are streamed.
                if (factory is RouteValueProviderFactory || factory is QueryStringValueProviderFactory)
                {
                    yield return factory;
                }
            }
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (_contentSelector.IsStreamedRequest(context.HttpContext))
            {
                var factories = context.ValueProviderFactories.ToList();
                context.ValueProviderFactories.Clear();

                foreach (var filteredFactory in FilterFactories(factories))
                    context.ValueProviderFactories.Add(filteredFactory);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }

    public class StreamedContentPolicySelector
    {
        PathString[] _streamedRequestRelativePaths;
        PathString[] _streamedResponseRelativePaths;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StreamedContentPolicySelector()
        {
            _streamedRequestRelativePaths = ArrayExtensions.EmptyArray<PathString>();
            _streamedResponseRelativePaths = ArrayExtensions.EmptyArray<PathString>();
        }

        /// <summary>
        /// Specifies which relatives paths should have a streamed input. (e.g. "/api/upload/")
        /// </summary>
        public IEnumerable<PathString> StreamedResquestRelativePaths
        {
            get
            {
                return _streamedRequestRelativePaths;
            }
            set
            {
                _streamedRequestRelativePaths = value.ToArray();
            }
        }

        /// <summary>
        /// Specifies which relatives paths should have a streamed output. (e.g. "/api/upload/")
        /// </summary>
        public IEnumerable<PathString> StreamedResponseRelativePaths
        {
            get
            {
                return _streamedResponseRelativePaths;
            }
            set
            {
                _streamedResponseRelativePaths = value.ToArray();
            }
        }

        public virtual bool IsStreamedRequest(HttpContext httpContext)
        {
            if (_streamedRequestRelativePaths.Length == 0)
                return false;

            for (var i = 0; i < _streamedRequestRelativePaths.Length; i++)
            {
                if (httpContext.Request.Path.StartsWithSegments(_streamedRequestRelativePaths[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public virtual bool IsStreamedResponse(HttpContext httpContext)
        {
            if (_streamedResponseRelativePaths.Length == 0)
                return false;

            for (var i = 0; i < _streamedResponseRelativePaths.Length; i++)
            {
                if (httpContext.Request.Path.StartsWithSegments(_streamedResponseRelativePaths[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
