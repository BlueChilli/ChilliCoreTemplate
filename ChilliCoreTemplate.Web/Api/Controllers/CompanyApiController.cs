using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Web.Controllers;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    public class CompaniesController : ControllerBase
    {
        ApiServices _service;
        Services _services;
        UserApiWebService _webService;

        public CompaniesController(ApiServices service, Services services, UserApiWebService webService)
        {
            _service = service;
            _services = services;
            _webService = webService;
        }

        /// <summary>
        /// Create company account
        /// </summary>
        [CustomAuthorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult Create(CompanyEditApiModel model)
        {
            return this.ApiServiceCall(() => _service.Company_Create(model)).Call();
        }

        /// <summary>
        /// Get company account
        /// </summary>
        [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
        [HttpGet("current")]
        [ProducesResponseType(typeof(CompanyApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Current()
        {
            return this.ApiServiceCall(() => _service.Company_Get()).Call();
        }

        /// <summary>
        /// Update company account
        /// </summary>
        [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
        [HttpPut("current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult Update(CompanyEditApiModel model)
        {
            return this.ApiServiceCall(() => _service.Company_Update(model)).Call();
        }

        [HttpPost]
        [Route("{id:int}/impersonate")]
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Impersonate(int id)
        {
            return this.ApiServiceCall(() => _services.Company_Impersonate(id, this.LoginWithPrincipal))
                .OnSuccess(x => { return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true)); })
                .Call();
        }

    }
}