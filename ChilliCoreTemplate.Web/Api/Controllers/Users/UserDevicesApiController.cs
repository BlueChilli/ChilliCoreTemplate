using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Web.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    [CustomAuthorize]
    public class UserDevicesController : ControllerBase
    {
        UserApiMobileService _svc;

        public UserDevicesController(UserApiMobileService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// Set pin for current logged on device
        /// </summary>
        [HttpPost("current/pin")]
        [ProducesResponseType(typeof(DevicePinResponseApiModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Add(PersistDevicePinApiModel model)
        {
            return await this.ApiServiceCall(() => _svc.SaveDevicePin(model)).Call();
        }

        /// <summary>
        /// Delete pin for current logged on device
        /// </summary>
        [HttpDelete("current/pin")]
        public async Task<IActionResult> Delete()
        {
            await _svc.DeleteDevicePin();

            return this.Ok(null);
        }

        /// <summary>
        /// Add pushtoken to users device
        /// </summary>
        [HttpPost]
        [Route("current/push")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddPush(PushTokenRegistrationApiModel model)
        {
            return await this.ApiServiceCall(() => _svc.RegisterPushToken(model))
                 .OnSuccess(x =>
                 {
                     return Ok();
                 })
                 .Call();
        }

        [HttpPost]
        [Route("current/push/test")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Test()
        {
            return await this.ApiServiceCall(() => _svc.TestPushNotification())
                 .OnSuccess(x =>
                 {
                     return Ok();
                 })
                 .Call();
        }

    }
}