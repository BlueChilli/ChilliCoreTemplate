using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api.PushNotifications;


internal class AWSPushNotification : PushNotificationApiServiceBase
{
    private AwsSnsConfiguration _config;
    private DataContext _context;

    public AWSPushNotification(AwsSnsConfiguration config, DataContext context)
    {
        _config = config;
        _context = context;
    }

    protected override bool IsSandBox(SendNotificationModel model)
    {
        var arn = string.IsNullOrEmpty(model.PushTokenId) ? _config.IOSArn : model.PushTokenId;

        if (!string.IsNullOrWhiteSpace(arn))
        {
            var components = arn.Split('/');
            return components.Any(m => m.ToUpper().Contains("APNS_SANDBOX"));
        }

        return true;
    }

    private string CreatePushNotificationMessage(SendNotificationModel model)
    {
        var data = this.CreateNativeMessage(model, model.Provider);
        var notification = data.ToJson(new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });

        object result;

        if (model.Provider == PushNotificationProvider.Google)
        {
            //TODO GCM to FCM
            result = new
            {
                GCM = notification
            };
        }
        else
        {
            if (!IsSandBox(model))
            {
                result = new
                {
                    @default = "",
                    APNS = notification
                };
            }
            else
            {
                result = new
                {
                    @default = "",
                    APNS_SANDBOX = notification
                };
            }

        }

        var ret = result.ToJson(Formatting.None);

        return ret;
    }

    private AmazonSimpleNotificationServiceClient CreateSNSClient()
    {
        return new AmazonSimpleNotificationServiceClient(_config.AccessKey, _config.Secret, Amazon.RegionEndpoint.APSoutheast2);
    }

    public async override Task<ServiceResult> SendPushNotification(SendNotificationModel model)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(model.PushTokenId)) return ServiceResult.AsError("Push token id is required");

            var snsClient = CreateSNSClient();

            var result = await snsClient.PublishAsync(new PublishRequest()
            {
                MessageStructure = "json",
                Message = CreatePushNotificationMessage(model),
                TargetArn = model.PushTokenId,
                Subject = model.Title
            });
        }
        catch (Exception ex)
        {
            if (ex is PlatformApplicationDisabledException)
            {
                Log.Logger.Error("Plaftform disabled for {User}, {Message}", model.UserId, model.Message);
            }
            if (!(ex is EndpointDisabledException) && !(ex is InvalidParameterException)) ex.LogException();

            return ex is EndpointDisabledException ? ServiceResult.AsSuccess() : ServiceResult.AsError(ex.Message);
        }
        return ServiceResult.AsSuccess();
    }

    public async override Task<ServiceResult> QueuePushNotification(SendNotificationModel model)
    {
        throw new NotImplementedException();
    }

    public override Task QueuePushNotificationTask(ITaskExecutionInfo executionInfo)
    {
        throw new NotImplementedException();
    }

    public override async Task<ServiceResult<string>> RegisterPushTokenToSNSAsync(string pushToken, PushNotificationProvider provider)
    {
        var snsClient = CreateSNSClient();
        string appArn;

        if (provider == PushNotificationProvider.Google)
        {
            appArn = _config.AndroidArn;
        }
        else
        {
            appArn = _config.IOSArn;
        }


        try
        {
            var request = await snsClient.CreatePlatformEndpointAsync(new CreatePlatformEndpointRequest { Token = pushToken, PlatformApplicationArn = appArn });

            if (String.IsNullOrEmpty(request.EndpointArn))
            {
                return ServiceResult<string>.AsError(error: $"Error registering push token: {pushToken}");
            }
            else
            {
                return ServiceResult<string>.AsSuccess(request.EndpointArn);
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error("Token failed to register: {PushToken}", pushToken);
            ex.LogException();
            return ServiceResult<string>.AsError(error: ex.Message);
        }
    }
}
