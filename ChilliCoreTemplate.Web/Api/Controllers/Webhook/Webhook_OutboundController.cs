using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ChilliCoreTemplate.Web.Api
{
    public partial class WebhooksController
    {

        [HttpPost]
        public IHttpActionResult Subscribe(WebhookApiModel model)
        {
            model.ApiKey = Guid.Parse(Request.Headers.GetValues("OrganisationApiKey").First());
            return ApiServiceCall(() => _service.Webhook_Subscribe(model)).Call();
        }

        [HttpPost]
        [Route("webhooks/unsubscribe")]
        public IHttpActionResult Unsubscribe(WebhookApiModel model)
        {
            return ApiServiceCall(() => _service.Webhook_Unsubscribe(model)).Call();
        }

        [HttpGet]
        [Route("webhooks/dogs/{id}")]
        //[ResponseType(typeof(DogExportApiModel))]
        public IHttpActionResult Dog_Export(int id)
        {
            var apiKey = Guid.Parse(Request.Headers.GetValues("OrganisationApiKey").First());
            //return ApiServiceCall(() => ApiServices.Dog_Export(id, apiKey)).Call();
            return Ok();
        }

        [HttpGet]
        [Route("webhooks/cats/{id}")]
        //[ResponseType(typeof(CatExportApiModel))]
        public IHttpActionResult Cat_Export(int id, bool isClean = false, int? vetId = null)
        {
            var apiKey = Guid.Parse(Request.Headers.GetValues("OrganisationApiKey").First());
            //return ApiServiceCall(() => ApiServices.Cat_Export(id, isClean, vetId, apiKey)).Call(modelIsRequired: false);
            return Ok();
        }

        [ApiAuthorization(Roles = new string[] { AccountCommon.Administrator })]
        [HttpGet]
        [Route("webhooks/trigger")]
        public IHttpActionResult Trigger()
        {
            //ApiServices.DailyTask_Run(null, runNow: true);
            return Ok();
        }


    }
}
