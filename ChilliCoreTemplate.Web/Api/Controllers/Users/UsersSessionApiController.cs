using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    public class UserSessionsController : ControllerBase
    {
        AccountService _accountService;
        UserApiMobileService _mobileService;
        UserApiWebService _webService;

        public UserSessionsController(AccountService accountService, UserApiMobileService mobileService, UserApiWebService webService)
        {
            _accountService = accountService;
            _mobileService = mobileService;
            _webService = webService;
        }

        /// <summary>
        /// Create a new session (login)
        /// </summary>
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        [HttpPost("byemail")]
        public virtual IActionResult AddByEmail(NewSessionApiModel model)
        {
            return this.ApiServiceCall(() => _webService.Login(model, model.Cookieless ? this.LoginWithPrincipalCookieless
                                                                                       : (Action<UserDataPrincipal>)this.LoginWithPrincipal))
                .OnSuccess((x) =>
                {
                    return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                })
                .Call();
        }

        /// <summary>
        /// Create a new session (login)
        /// </summary>
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        [HttpPost("bytoken/{token}")]
        public virtual IActionResult AddByToken(EmailTokenModel model)
        {
            return this.ApiServiceCall(() => _accountService.LoginWithToken(model, this.LoginWithPrincipal))
                .OnSuccess(x =>
                {
                    return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                })
                .Call();
        }

        ///// <summary>
        ///// Create a new session (login)
        ///// </summary>
        //[ProducesResponseType(typeof(LoginCodeResponseApiModel), StatusCodes.Status200OK)]
        //[HttpPost("byphone/coderequest")]
        //public IActionResult CodeRequest([FromBody]LoginCodeRequestApiModel model)
        //{
        //    return this.ApiServiceCall(() => _mobileService.RequestLoginCode(model)).Call();
        //}

        //[ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        //[HttpPost("byphone")]
        //public IActionResult AddByPhone([FromBody]NewSessionByPhoneApiModel model)
        //{
        //    return this.ApiServiceCall(() => _mobileService.LoginWithPhoneNumber(model, this.LoginWithPrincipal))
        //        .OnSuccess(x =>
        //        {
        //            return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
        //        })
        //        .Call();
        //}

        [ProducesResponseType(typeof(LoginResultMobileModel), StatusCodes.Status200OK)]
        [HttpPost("bypin")]
        public virtual IActionResult AddByPin(PinLoginPinApiModel model)
        {
            return this.ApiServiceCall(() => _mobileService.LoginWithPin(model, this.LoginWithPrincipalCookieless))
                 .OnSuccess(x =>
                 {
                     return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                 })
                .Call();
        }

        /// <summary>
        /// Current session details
        /// </summary>
        [CustomAuthorize]
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        [HttpGet("current")]
        public virtual IActionResult Current()
        {
            return Ok(_webService.GetSessionSummary());
        }

        /// <summary>
        /// Delete session (logout)
        /// </summary>
        [HttpDelete("current")]
        public async virtual Task<IActionResult> Delete()
        {
            await this.LogoutPrincipalAsync();
            return this.Ok(null);
        }

        /// <summary>
        /// Impersonate another user
        /// </summary>
        [HttpPost("impersonate")]
        [CustomAuthorize(MultipleRoles = new string[] { AccountCommon.Administrator, AccountCommon.CompanyAdmin })]
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Impersonate(int id)
        {
            return this.ApiServiceCall(() => _accountService.ImpersonateAccount(id, this.LoginWithPrincipal))
                .OnSuccess(x =>
                {
                    return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                })
                .Call();
        }

        /// <summary>
        /// Undo impersonation and return to previous session
        /// </summary>        
        [HttpPost("undoimpersonate")] //Not idempotent, we should use post.
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult UndoImpersonate()
        {
            return this.ApiServiceCall(() => _accountService.RemoveImpersonation(this.LoginWithPrincipal))
                .OnSuccess(x =>
                {
                    return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                })
                .Call();
        }
    }
}