using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/companies/current/users")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
    public class CompanyUsersController : ControllerBase
    {
        CompanyApiService _service;
        UserApiWebService _webApiService;

        public CompanyUsersController(CompanyApiService service, UserApiWebService webApiService)
        {
            this._webApiService = webApiService;
            this._service = service;
        }

        /// <summary>
        /// List company employees
        /// </summary>
        [HttpGet()]
        [ProducesResponseType(typeof(List<CompanyUserApiModel>), StatusCodes.Status200OK)]
        public virtual IActionResult List()
        {
            return this.ApiServiceCall(_service.Admin_List)
                .Call();
        }

        /// <summary>
        /// Get company employee
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CompanyUserApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Get(int id)
        {
            return this.ApiServiceCall(() => _service.Admin_Get(id))
                .Call();
        }


        /// <summary>
        /// Invite company employee
        /// </summary>
        [HttpPost()]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Invite([FromBody] InviteEditApiModel model)
        {
            return this.ApiServiceCall(() => _webApiService.Invite(model))
                .Call();
        }

        /// <summary>
        /// Update company employee
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult Update(CompanyUserEditApiModel model)
        {
            return this.ApiServiceCall(() => _service.Admin_Update(model))
                .Call();
        }

        /// <summary>
        /// Remove company employee
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult Delete(int id)
        {
            return this.ApiServiceCall(() => _service.Admin_Delete(id))
                .Call();
        }

    }
}
