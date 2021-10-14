
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;

namespace ChilliCoreTemplate.Service.Api
{
    //public partial class WebhookService
    //{
    //    internal ServiceResult<ApiGetAccountRequest> Account_Get(Guid apiKey)
    //    {
    //        //TODO
    //        //var map = LinqMapper.GetMap<Organisation, OrganisationApiModel>();

    //        //var result = Context.Organisations.Where(o => o.ApiKey == apiKey)
    //        //                    .Select(map).FirstOrDefault();
    //        //if (result == null) return ServiceResult<OrganisationApiModel>.AsError("Not authorised", System.Net.HttpStatusCode.Unauthorized);

    //        return ServiceResult<ApiGetAccountRequest>.AsSuccess(new ApiGetAccountRequest { UserId = 1 });
    //    }

    //    public ServiceResult<WebhookApiModel> Webhook_Subscribe(WebhookApiModel model)
    //    {
    //        var organisation = Account_Get(model.ApiKey);  //TODO change as appropriate
    //        if (!organisation.Success) return ServiceResult<WebhookApiModel>.CopyFrom(organisation);

    //        if (Context.Webhooks_Outbound.Any(h => h.Target_Url == model.Target_Url)) return ServiceResult<WebhookApiModel>.AsError("Webhook already subscribed", HttpStatusCode.Conflict);

    //        var webHook = Context.Webhooks_Outbound.Add(new Webhook_Outbound
    //        {
    //            OrganisationId = organisation.Result.UserId.Value,
    //            Target_Url = model.Target_Url,
    //            Event = model.Event
    //        });
    //        Context.SaveChanges();

    //        return ServiceResult<WebhookApiModel>.AsSuccess(new WebhookApiModel { Id = webHook.Id });
    //    }

    //    public ServiceResult Webhook_Unsubscribe(WebhookApiModel model)
    //    {
    //        var webhook = Context.Webhooks_Outbound.FirstOrDefault(h => h.Target_Url == model.Target_Url);

    //        if (webhook == null) return ServiceResult.AsError("Webhook not found", HttpStatusCode.NotFound);

    //        Context.Webhooks_Outbound.Remove(webhook);
    //        Context.SaveChanges();

    //        return ServiceResult.AsSuccess();
    //    }

    //    /// <summary>
    //    /// Trigger subscribed webhooks
    //    /// </summary>
    //    /// <param name="organisationId"></param>
    //    /// <param name="type"></param>
    //    /// <param name="id"></param>
    //    /// <param name="data"></param>
    //    /// <returns></returns>
    //    internal ServiceResult Webhook_Trigger(int organisationId, WebhookEvent type, int id, object data)
    //    {
    //        try
    //        {
    //            if (!WebhookServiceConfiguration.GetConfig().Enabled) return ServiceResult.AsSuccess();

    //            var webhooks = Context.Webhooks_Outbound.Where(h => h.OrganisationId == organisationId && h.Event == type).ToList();

    //            foreach (var webhook in webhooks)
    //            {
    //                var client = new RestSharp.RestClient(webhook.Target_Url);
    //                var request = new RestSharp.RestRequest(RestSharp.Method.POST);
    //                request.AddParameter("resource_url", webhook.ResourceUrl(id, data));
    //                var response = client.Execute(request);
    //                if (response.StatusCode == HttpStatusCode.Gone)
    //                {
    //                    Context.Webhooks_Outbound.Remove(webhook);
    //                    Context.SaveChanges();
    //                }
    //            }

    //            return ServiceResult.AsSuccess();
    //        }
    //        catch (Exception ex)
    //        {
    //            ex.LogException();
    //            return ServiceResult.AsError(ex.Message);
    //        }
    //    }

    //    /// <summary>
    //    /// Trigger webhook manually
    //    /// </summary>
    //    /// <param name="obj"></param>
    //    /// <param name="targetUrl"></param>
    //    /// <returns></returns>
    //    public ServiceResult Webhook_Trigger(object obj, string targetUrl = null)
    //    {
    //        try
    //        {
    //            if (obj == null)
    //                throw new ArgumentNullException("Webhook object can not be null.");

    //            var config = WebhookServiceConfiguration.GetConfig();
    //            if (config == null || !config.Enabled)
    //                return ServiceResult.AsSuccess();

    //            var jsonOptions = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
    //            var zapierJson = JsonConvert.SerializeObject(obj, jsonOptions);

    //            var client = new RestSharp.RestClient(targetUrl ?? config.TargetURL);
    //            var request = new RestSharp.RestRequest("", RestSharp.Method.POST);
    //            request.Parameters.Add(new RestSharp.Parameter() { Name = "Content-Type", Value = "application/json", Type = RestSharp.ParameterType.HttpHeader });
    //            request.Parameters.Add(new RestSharp.Parameter() { Name = "Accept", Value = "application/json", Type = RestSharp.ParameterType.HttpHeader });

    //            request.Parameters.Add(new RestSharp.Parameter() { Value = zapierJson, Type = RestSharp.ParameterType.RequestBody });

    //            var response = client.Execute(request);
    //            if (response.StatusCode != HttpStatusCode.OK)
    //            {
    //                var errorMsg = $"Webhook post error: {response.StatusCode}";
    //                ErrorLogHelper.LogMessage(errorMsg);
    //                return ServiceResult.AsError(errorMsg);
    //            }

    //            return ServiceResult.AsSuccess();
    //        }
    //        catch (Exception ex)
    //        {
    //            ex.LogException();
    //            return ServiceResult.AsError(error: ex.Message);
    //        }
    //    }

    //}


    public class ZapierMessage
    {
        public ZapierMessage(string message)
        {
            this.Message = message;
        }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

}
