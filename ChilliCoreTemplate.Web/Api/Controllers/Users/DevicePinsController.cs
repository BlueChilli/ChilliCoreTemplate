using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    [CustomAuthorize]
    public class DevicePinsController : ControllerBase
    {
        UserApiMobileService _svc;

        public DevicePinsController(UserApiMobileService svc)
        {
            _svc = svc;
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(DevicePinResponseApiModel), StatusCodes.Status200OK)]
        public IActionResult Add(PersistDevicePinApiModel model)
        {
            return this.ApiServiceCall(() => _svc.SaveDevicePin(model)).Call();
        }

        [HttpDelete("")]
        public IActionResult Delete()
        {
            _svc.DeleteDevicePin();

            return this.Ok(null);
        }
    }
}