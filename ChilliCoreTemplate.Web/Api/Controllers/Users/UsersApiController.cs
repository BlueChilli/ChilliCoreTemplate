using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    public class UsersController : ControllerBase
    {
        AccountService _accountService;
        UserApiWebService _webApiService;
        UserApiMobileService _mobileApiService;

        public UsersController(AccountService accountService, UserApiWebService webApiService, UserApiMobileService mobileApiService)
        {
            this._accountService = accountService;
            this._webApiService = webApiService;
            this._mobileApiService = mobileApiService;
        }

        /// <summary>
        /// Create a user (registration)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("byemail")]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult AddByEmail(RegistrationApiModel model)
        {
            return this.ApiServiceCall(() => _webApiService.Create(model))
                .Call();
        }

        /// <summary>
        /// Create a user (registration)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("byphone")]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult AddByPhone(PhoneRegistrationApiModel model)
        {
            return this.ApiServiceCall(() => _mobileApiService.Create(model))
                .Call();
        }

        /// <summary>
        /// Create a token 
        /// 1. password - delivered by email with link to ~/users/resetpassword)
        /// 2. activate - delivered by email with link to ~/users/confirmemail
        /// 5. onetimepassword - delivered by email
        /// </summary>
        [AllowAnonymous]
        [HttpPost("tokens")]
        public virtual IActionResult CreateToken(TokenEditApiModel model)
        {
            return this.ApiServiceCall(() => _webApiService.Token_Create(model)).Call();
        }

        /// <summary>
        /// Get user by token. (Will also activate account)
        /// </summary>
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        [HttpGet("bytoken/{token}")]
        public virtual IActionResult GetByToken(UserTokenModel model)
        {
            return this.ApiServiceCall(() => _webApiService.GetByToken(model)).Call();
        }

        /// <summary>
        /// Get user by code. (Will also activate account)
        /// </summary>
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        [HttpGet("bycode/{token}")]
        public virtual IActionResult GetByCode(UserTokenModel model)
        {
            return this.ApiServiceCall(() => _webApiService.GetByCode(model)).Call();
        }

        /// <summary>
        /// Patches user by token (update password and/or name).
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPatch("bytoken/{token}")]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult PatchByToken(PatchAccountTokenApiModel model)
        {
            return this.ApiServiceCall(() => _webApiService.PatchUser(model)).Call();
        }

        //[HttpGet]
        //[ResponseType(typeof(UserAccountApiModel))]
        //[Route("current")]
        //public IActionResult QuerySync()
        //{
        //    var userData = User.UserData();
        //    return this.ApiServiceCall(() => _webApiService.GetAccount(userData.UserId)).Call();
        //}

        /// <summary>
        /// Current user
        /// </summary>
        [CustomAuthorize]
        [HttpGet("current")]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        public async virtual Task<IActionResult> Query()
        {
            var userData = User.UserData();
            return await this.ApiServiceCall(() => _webApiService.GetAccountAsync(userData?.UserId)).Call();
        }

        /// <summary>
        /// Patch user (password and/or email and/or name and status (eg change anonymous to registered or change status to deleted)
        /// </summary>
        [CustomAuthorize]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        [HttpPatch("current")]
        public virtual IActionResult Patch(PatchAccountApiModel model)
        {
            var userData = User.UserData();
            model.Id = userData.UserId;

            return this.ApiServiceCall(() => _webApiService.PatchUser(model)).Call();
        }

        /// <summary>
        /// Update user details (Firstname, Lastname, Email, Profile photo (optional) as form post
        /// </summary>
        [CustomAuthorize]
        [HttpPut("current")]
        [ProducesResponseType(typeof(UserAccountApiModel), StatusCodes.Status200OK)]
        public virtual IActionResult Update(PersistUserAccountApiModel model)
        {
            var userData = User.UserData();
            model.Id = userData.UserId;

            return this.ApiServiceCall(() => _webApiService.Update(model)).Call();
        }

        /// <summary>
        /// Delete account
        /// </summary>
        [HttpDelete]
        [Route("current")]
        [CustomAuthorize]
        public async Task<IActionResult> Delete(DeleteUserApiModel model)
        {
            var userData = User.UserData();
            return await this.ApiServiceCall(async () =>
            {
                model.Id = userData.UserId;
                return await _webApiService.Delete(model);
            }).Call();
        }

        /// <summary>
        /// Add pushtoken to users device
        /// </summary>
        [HttpPost]
        [Route("push")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddPush(PushTokenRegistrationApiModel model)
        {
            return await this.ApiServiceCall(() => _mobileApiService.RegisterPushToken(model))
                 .OnSuccess(x =>
                 {
                     return Ok();
                 })
                 .Call();
        }

    }
}
