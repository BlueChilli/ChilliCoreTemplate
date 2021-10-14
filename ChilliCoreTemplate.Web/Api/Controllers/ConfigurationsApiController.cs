using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace ChilliCoreTemplate.Web.Api
{

    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    public class ConfigurationsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ProjectSettings _config;

        public ConfigurationsController(IWebHostEnvironment environment, ProjectSettings config)
        {
            _environment = environment;
            _config = config;
        }

        /// <summary>
        /// For apple to test the live app against a testing endpoint. So the app can be tested live before the api is pushed to production.
        /// Current: App live version 1.1.3, App submission version 1.1.3 , API redirect target 1.1.4 => no redirect
        /// Submit app for approval: App live version 1.1.3, App submission version 1.1.4 , API redirect target 1.1.4 => staging redirect for submission version
        /// App and BE pushed live: App live version 1.1.4, App submission version 1.1.4 , API redirect target 1.1.5 => no redirect
        /// </summary>
        /// <returns></returns>
        [HttpGet("endpoint/{version?}")]
        [ProducesResponseType(typeof(EndPointApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Endpoint(string version = null, AppType appType = AppType.Default)
        {
            var model = new EndPointApiModel
            {
                AppType = appType,
                CurrentVersion = version,
                TestVersion = appType.GetData<string>("TestVersion"),
                Environment = _environment.EnvironmentName,
                Application = _config.ProjectName,
                EndPoint = _config.BaseUrl.Replace("http://", "https://") + "/"
            };
            if (_environment.IsProduction())
            {
                if (model.IsRedirect)
                {
                    model.EndPoint = model.EndPoint.Replace("app", "staging");
                }
            }
            return Ok(model);
        }
    }

    public class EndPointApiModel
    {
        public string Application { get; set; }

        public string Environment { get; set; }

        public string EndPoint { get; set; }

        public AppType AppType { get; set; }

        public string CurrentVersion { get; set; }

        public string TestVersion { get; set; }

        public bool IsRedirect => CurrentVersion == TestVersion;

    }

    public enum AppType
    {
        [Data("TestVersion", "0.0.0")]
        Default,
        [Data("TestVersion", "0.0.0")]
        Other
    }
}