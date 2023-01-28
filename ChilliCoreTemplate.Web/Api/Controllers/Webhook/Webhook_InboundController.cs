using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.Api;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChilliCoreTemplate.Web.Api
{
    [ApiKeyIgnore]
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    public partial class WebhooksController : ControllerBase
    {
        WebhookService _service;
        ProjectSettings _config;
        IWebHostEnvironment _env;

        public WebhooksController(WebhookService service, ProjectSettings config, IWebHostEnvironment env)
        {
            _service = service;
            _config = config;
            _env = env;
        }

        [CustomAuthorize(MultipleRoles = new string[] { AccountCommon.Administrator })]
        [HttpGet]
        [Route("run")]
        public void Run(string webhookId)
        {
            _service.ProcessWebhook(webhookId);
        }
    }
}
