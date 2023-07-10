using ChilliCoreTemplate.Models.Api;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public partial class WebhooksController
    {
        [ApiKeyIgnore]
        [HttpPost]
        [Route("stripe")]
        public IActionResult Stripe([FromBody]object json)
        {
            var result = _service.QueueWebhook(WebhookType.Stripe, json.ToJson());
            return Ok(result.Error);
        }

        [ApiKeyIgnore]
        [HttpPost]
        [Route("stripecreate")]
        public IActionResult Webhook(Guid secret)
        {
            _service.Stripe_CreateWebhook(secret);
            return Ok();
        }

    }
}
