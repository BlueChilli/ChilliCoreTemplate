using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class TemplateViewRenderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TemplateServiceException"/>
        /// </summary>
        /// <param name="message"></param>
        public TemplateViewRenderException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TemplateServiceException"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TemplateViewRenderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public interface ITemplateViewRenderer
    {
        /// <summary>
        /// Render razor template in given by filename
        /// </summary>
        /// <param name="viewName">razor view name</param>
        /// <param name="viewModel"></param>
        /// <typeparam name="TViewModel"></typeparam>
        /// <remarks>template view is searched using Asp.net core view location. ie uses ViewEngine.FindView method</remarks>
        /// <example>RenderAsync("TestView", model>)</example>
        /// <returns></returns>
        Task<string> RenderAsync<TViewModel>(string viewName, TViewModel viewModel);


    }

    public class TemplateViewRendererOptions
    {
        public Uri BaseUri { get; set; }
    }

    //Based from https://github.com/scottsauber/RazorHtmlEmails/blob/master/src/RazorHtmlEmails.RazorClassLib/Services/RazorViewToStringRenderer.cs
    public class TemplateViewRenderer : ITemplateViewRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly TemplateViewRendererOptions _options;

        public TemplateViewRenderer(IRazorViewEngine viewEngine, IServiceProvider serviceProvider, ITempDataProvider tempDataProvider, IOptions<TemplateViewRendererOptions> options)
        {
            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
            _options = options.Value;
        }

        public async Task<string> RenderAsync<TViewModel>(string viewName, TViewModel viewModel)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewName);

            var viewDictionary = new ViewDataDictionary<TViewModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = viewModel
            };

            var tempDataDictionary = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            try
            {
                using (var outputWriter = new StringWriter())
                {
                    var viewContext = new ViewContext(actionContext, view, viewDictionary,
                        tempDataDictionary, outputWriter, new HtmlHelperOptions());

                    await view.RenderAsync(viewContext);

                    return outputWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new TemplateViewRenderException($"Failed to render template ({viewName}) due to a razor engine failure", ex);
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Failed to render template {viewName} because it was not found in paths:" }.Concat(searchedLocations)); ;

            throw new TemplateViewRenderException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            if (_options?.BaseUri != null)
            {
                httpContext.Request.Scheme = _options.BaseUri.Scheme;
                httpContext.Request.Host = HostString.FromUriComponent(_options.BaseUri);
                httpContext.Request.PathBase = PathString.FromUriComponent(_options.BaseUri);
            }

            var router = _serviceProvider.GetService<IMvcRouterAccessor>()?.Router;
            var routeData = router == null ? new RouteData() : new RouteData() { Routers = { router } };
            return new ActionContext(httpContext, routeData, new ActionDescriptor());
        }
    }
}