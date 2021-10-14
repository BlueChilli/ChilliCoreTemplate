using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.Sms;
using ChilliCoreTemplate.Web.Api.Library;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public partial class WebhooksController
    {
        [ApiKeyIgnore]
        [HttpPost]
        [Route("twilio/status")]
        [ValidateTwilioRequest]
        public IActionResult Status(TwilioSmsStatusModel model)
        {
            model.Type = TwilioSmsType.Status;
            var json = model.ToJson();
            _service.QueueWebhook(WebhookType.Twilio, json);

            Request.Headers.TryGetValue("X-Twilio-Signature", out StringValues signature);

            var message = _env.IsProduction() ? "Message queued for processing" : signature.FirstOrDefault();
            var xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response><!--{message}--></Response>";

            return Content(xml, "text/xml", Encoding.UTF8);
        }

        //[ApiKeyIgnore]
        //[HttpPost]
        //[Route("webhooks/twilio/message")]
        //[ValidateTwilioRequest]
        //public async Task<HttpResponseMessage> Message(TwilioSmsMessageModel model)
        //{
        //    Request.Headers.TryGetValue("X-Twilio-Signature", out StringValues signatureHeaders);
        //    var signature = ProjectConfigurationSection.GetConfig().ProjectEnvironment == ProjectEnvironment.Development ? $"<!--{signatureHeaders?.FirstOrDefault()}-->" : "";

        //    var message = Constants.OptOutMessage;
        //    if (model.Command == TwilioMessageCommand.Stop) message = "You have opted out. Send START to opt in.";
        //    else if (model.Command == TwilioMessageCommand.Start) message = "You have opted in. Send STOP to opt out.";

        //    var xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response>{signature}<Message>{message}</Message></Response>";

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(xml, Encoding.UTF8, "text/xml");

        //    if (model.Command != TwilioMessageCommand.None)
        //    {
        //        model.Type = TwilioSmsType.Message;
        //        var json = model.ToJson();
        //        _service.QueueWebhook(WebhookType.Twilio, json);
        //    }

        //    return response;
        //}
    }
}
