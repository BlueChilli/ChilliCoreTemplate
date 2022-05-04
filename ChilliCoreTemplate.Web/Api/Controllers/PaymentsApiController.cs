using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Web.Api
{
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(MultipleRoles = new string[] { AccountCommon.User, AccountCommon.CompanyAdmin })]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentApiService _service;

        public PaymentsController(PaymentApiService service)
        {
            _service = service;
        }

        // <summary>
        /// Update payment configuration for this account eg coupon / credit card associated with the account
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(typeof(PaymentDetailApiModel), StatusCodes.Status200OK)]
        [Route("configuration")]
        public virtual IActionResult Edit(PaymentEditApiModel model)
        {
            return this.ApiServiceCall(() => _service.Edit(model)).Call();
        }

        /// <summary>
        /// Current payment configuration and first page of payments
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaymentDetailApiModel), StatusCodes.Status200OK)]
        [Route("configuration")]
        public virtual IActionResult Detail()
        {
            return this.ApiServiceCall(() => _service.Detail()).Call();
        }

        /// <summary>
        /// Cancel configured subscription
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(PaymentDetailApiModel), StatusCodes.Status200OK)]
        [Route("configuration")]
        public virtual IActionResult Delete(SubscriptionDeleteApiModel model)
        {
            return this.ApiServiceCall(() => _service.Delete()).Call();
        }

        /// <summary>
        /// List of Payments (paging)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiPagedList<PaymentApiModel>), StatusCodes.Status200OK)]
        [Route("")]
        public virtual IActionResult List([FromQuery]ApiPaging paging = null)
        {
            return this.ApiServiceCall(() => _service.List(paging)).Call();
        }
    }
}