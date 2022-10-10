using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.Api.OAuth;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Web.Controllers;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Net;
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
        ProjectSettings _config;

        public UserSessionsController(AccountService accountService, UserApiMobileService mobileService, UserApiWebService webService, ProjectSettings config)
        {
            _accountService = accountService;
            _accountService.IsApi = true;
            _mobileService = mobileService;
            _webService = webService;
            _config = config;
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
        public virtual IActionResult AddByToken(UserTokenModel model)
        {
            return this.ApiServiceCall(() => _accountService.LoginWithToken(model, this.LoginWithPrincipal))
                .OnSuccess(x =>
                {
                    return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                })
                .Call();
        }

        /// <summary>
        /// Create a new session from an OTP code
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        [HttpPost("bycode/{token}")]
        public virtual IActionResult AddByCode(UserTokenModel model)
        {
            return this.ApiServiceCall(() => _accountService.LoginWithCode(model, this.LoginWithPrincipal))
                 .OnSuccess(x =>
                 {
                     return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                 })
                .Call();
        }

        /// <summary>
        /// Request OAuth access for login. RedirectUrl is the url the login token or errors will be sent to.
        /// </summary>
        [ApiKeyIgnore]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [HttpGet("byoauth/{provider:oAuthProvider}")]
        public virtual IActionResult AddByOAuthUrl(OAuthUrlApiModel model)
        {
            model.Email = User?.UserData()?.Email;
            return this.ApiServiceCall(() => _accountService.OAuth_Url(model, OAuthMode.Login))
                .OnSuccess(x =>
                {
                    return Redirect(x.Result);
                })
                .Call();
        }

        /// <summary>
        /// Request OAuth access for login for an existing account. RedirectUrl is the url the login token or errors will be sent to.
        /// </summary>
        [CustomAuthorize]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [HttpPost("byoauth/{provider:oAuthProvider}/link")]
        public virtual IActionResult AddByOAuthUrlLink(OAuthUrlApiModel model)
        {
            model.Email = User?.UserData()?.Email;
            return this.ApiServiceCall(() => _accountService.OAuth_Url(model, OAuthMode.Login))
                .OnSuccess(x =>
                {
                    return Redirect(x.Result);
                })
                .Call();
        }

        /// <summary>
        /// Exchange OAuth access token for authentication with cookie or userkey. If account exists and oauth provider has not already been linked the user must be authenticated to link accounts with oauth.
        /// </summary>
        [HttpPost("byoauth/{provider:oAuthProvider}")]
        [ProducesResponseType(typeof(SessionSummaryApiModel), StatusCodes.Status200OK)]
        public async virtual Task<IActionResult> AddByOAuth(OAuthLoginApiModel model)
        {
            return await this.ApiServiceCall(() => _accountService.OAuth_Login(model, model.Cookieless ? this.LoginWithPrincipalCookieless : (Action<UserDataPrincipal>)this.LoginWithPrincipal))
                .OnSuccess(x =>
                {
                    return Ok(_webService.GetSessionSummary(x.Result, includeUserKey: true));
                })
                .Call();
        }

        /// <summary>
        /// OAuth response from provider. Redirect to nominated url with a token for login.
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiKeyIgnore]
        [HttpGet("byoauth/auth")]
        public async virtual Task<IActionResult> AddByOAuthCode(OAuthCodeApiModel model)
        {
            if (model == null) return Ok();
            var redirectUrl = _config.ResolveApiUrl(String.IsNullOrEmpty(model.State) ? _config.OAuthsSettings.DefaultUrl : model.State.Split('|')[2]);
            return await this.ApiServiceCall(() => _accountService.OAuth_Code(model))
                .OnSuccess(x =>
                {
                    return Redirect($"{redirectUrl}?token={x.Result.Token.ToShortGuid()}&email={WebUtility.UrlEncode(x.Result.Email)}");
                })
                .OnFailure(x => { return Redirect($"{redirectUrl}?error={x.Key}&errordescription={x.Error}"); })
                .Call();
        }

        /// <summary>
        /// OAuth response from provider. Redirect to nominated url with a token for login.
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiKeyIgnore]
        [HttpPost("byoauth/auth")]
        public async virtual Task<IActionResult> AddByOAuthCodePost(OAuthCodeApiModel model)
        {
            return await AddByOAuthCode(model);
        }

        /// <summary>
        /// Request to remove authentication (facebook tests if this method exists during setup)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiKeyIgnore]
        [HttpGet("byoauth/deauth/{provider:oAuthProvider}")]
        public virtual IActionResult RemoveByOAuth_OK(OAuthProvider provider)
        {
            return Ok();
        }

        /// <summary>
        /// Request to remove authentication (facebook tests if this method exists during app configuration)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiKeyIgnore]
        [HttpPost("byoauth/deauth/{provider:oAuthProvider}")]
        public virtual IActionResult RemoveByOAuth(OAuthDeauthApiModel model)
        {
            return this.ApiServiceCall(() => _accountService.OAuth_Deauth(model))
                .OnSuccess(x =>
                {
                    return Ok();
                })
                .OnFailure(x =>
                {
                    Log.Error("Oauth deauth failed {0}. Data: {1}", x.Error, model.ToJson());
                    return Ok();
                })
                .Call();
        }

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