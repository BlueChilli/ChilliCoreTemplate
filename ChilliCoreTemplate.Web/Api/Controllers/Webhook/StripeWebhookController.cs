using ChilliCoreTemplate.Models.Api;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api;

public partial class WebhooksController
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiKeyIgnore]
    [HttpPost]
    [Route("stripe")]
    public async Task<IActionResult> Stripe()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"];
        var result = await _service.QueueWebhook(WebhookType.Stripe, json, signatureHeader);
        return Ok(result.Error);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiKeyIgnore]
    [HttpPost]
    [Route("stripecreate")]
    public IActionResult Webhook(Guid secret)
    {
        _service.Stripe_CreateWebhook(secret);
        return Ok();
    }

}
