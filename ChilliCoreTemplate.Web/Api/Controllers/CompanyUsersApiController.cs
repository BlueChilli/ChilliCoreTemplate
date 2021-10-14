using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChilliCoreTemplate.Web.Api;
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
        ApiServices _services;
        UserApiWebService _webApiService;

        public CompanyUsersController(ApiServices services, UserApiWebService webApiService)
        {
            this._webApiService = webApiService;
            this._services = services;
        }

        /// <summary>
        /// List company employees
        /// </summary>
        [HttpGet()]
        [ProducesResponseType(typeof(List<CompanyUserApiModel>), StatusCodes.Status200OK)]
        public virtual IActionResult List()
        {
            return this.ApiServiceCall(() => _services.Company_Admin_List())
                .Call();
        }

        /// <summary>
        /// Get company employee
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CompanyUserApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Get(int id)
        {
            return this.ApiServiceCall(() => _services.Company_Admin_Get(id))
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
            return this.ApiServiceCall(() => _services.Company_Admin_Update(model))
                .Call();
        }

        /// <summary>
        /// Remove company employee
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult Delete(int id)
        {
            return this.ApiServiceCall(() => _services.Company_Admin_Delete(id))
                .Call();
        }

    }
}
