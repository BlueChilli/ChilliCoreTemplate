using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.Api.FireBase;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.Azure.NotificationHubs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formatting = Newtonsoft.Json.Formatting;

namespace ChilliCoreTemplate.Service.Api
{
    public class PushNotificationConfiguration : IService
    {
        private PushNotificationSettings _settings;
        private DataContext _context;

        public PushNotificationConfiguration(PushNotificationSettings settings, DataContext context)
        {
            _settings = settings;
            _context = context;
        }

        public PushNotificationApiService GetService(PushNotificationAppId appId = PushNotificationAppId.Default)
        {
            var app = _settings.Apps.Where(x => x.AppId == appId).FirstOrDefault();
            if (app == null)
            {
                throw new ApplicationException($"Push configuration not found for app {appId}");
            }

            if (app.Aws != null) return new AWSPushNotification(app.Aws, _context);
            if (app.Azure != null) return new AzurePushNotification(app.Azure, _context);
            if (app.FireBase != null) return new FireBasePushNotification(app.FireBase, _context);
            throw new ApplicationException($"Push configuration incomplete for app {appId}");
        }

        public async Task<ServiceResult> SendPushNotification(SendNotificationModel model)
        {
            var service = GetService(model.AppId);
            return await service.SendPushNotification(model);
        }

        public async Task<ServiceResult> QueuePushNotification(SendNotificationModel model)
        {
            var service = GetService(model.AppId);
            return await service.QueuePushNotification(model);
        }
    }

    public abstract class PushNotificationApiService
    {
        /// <summary>
        /// register push notification token to SNS
        /// </summary>        
        /// <param name="pushToken">push notification token given from mobile device</param>
        /// <param name="provider">prodiver of push notification service (Google or Apple)</param>
        /// <remarks>
        /// use SnsPlatformApplicationArnGoogle as Google's SNS arn in config key
        /// use SnsPlatformApplicationArn as Apple's SNS arn in config
        /// </remarks>
        /// <returns>SNS token Id</returns>
        public abstract Task<ServiceResult<string>> RegisterPushTokenToSNSAsync(string pushToken, PushNotificationProvider provider);

        /// <summary>
        /// sends push notification via SNS
        /// </summary>
        /// <param name="pushTokenId">SNS token Id</param>
        /// <param name="subject">title of the push notification message</param>
        /// <param name="type">type of push notification</param>
        /// <param name="message">message</param>
        /// <param name="data">additonal data</param>
        /// <returns>Asynchronous Task</returns>
        public abstract Task<ServiceResult> SendPushNotification(SendNotificationModel model);

        /// <summary>
        /// queue push notification via SNS
        /// </summary>
        public abstract Task<ServiceResult> QueuePushNotification(SendNotificationModel model);

        public abstract Task QueuePushNotificationTask(ITaskExecutionInfo executionInfo);

        /// <summary>
        /// check whether apple push notification server is sandbox server
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsSandBox();

        protected object CreateNativeMessage(SendNotificationModel model, PushNotificationProvider provider)
        {
            switch (provider)
            {
                case PushNotificationProvider.Google:

                    var flutter = new Dictionary<string, string> { ["click_action"] = "FLUTTER_NOTIFICATION_CLICK" };
                    if (model.Data == null) model.Data = flutter;
                    else model.Data.AddOrSkipIfExists("click_action", "FLUTTER_NOTIFICATION_CLICK");

                    object googleMessage = new
                    {
                        notification = new { title = model.Title, body = model.Message, sound = model.Sound, badge = model.BadgeCount },
                        data = model.Data,
                        mode = (int)model.Type
                    };

                    return googleMessage;
                default:
                    object defaultMessage = new
                    {
                        aps = new { alert = new { title = model.Title, body = model.Message }, sound = model.Sound, badge = model.BadgeCount },
                        mode = (int)model.Type
                    };

                    if (model.Data != null)
                    {
                        //TODO
                        //defaultMessage = TypeMerger.MergeTypes(defaultMessage, model.Data);
                    }

                    return defaultMessage;
            }
        }

        internal async Task SendToAccountIds(List<int> accountIds, AccountService accountSvc, string message, Dictionary<string, string> data = null)
        {
            var devices = accountSvc.UserDevice_List(accountIds);

            foreach (var device in devices)
            {
                var model = new SendNotificationModel
                {
                    UserId = device.UserId,
                    PushTokenId = device.TokenId,
                    Provider = device.Provider,
                    Type = PushNotificationType.Test,
                    Title = null,
                    Message = message,
                    Data = data
                };
                await SendPushNotification(model);
            }
        }
    }

    internal class AWSPushNotification : PushNotificationApiService
    {
        private AwsSnsConfiguration _config;
        private DataContext _context;

        public AWSPushNotification(AwsSnsConfiguration config, DataContext context)
        {
            _config = config;
            _context = context;
        }

        protected override bool IsSandBox()
        {
            var arn = _config.IOSArn;

            if (!String.IsNullOrWhiteSpace(arn))
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
                if (!IsSandBox())
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
                Serilog.Log.Logger.Error(ex, "Token failed to register: {PushToken}", pushToken);
                ex.LogException();
                return ServiceResult<string>.AsError(error: ex.Message);
            }
        }
    }

    internal class AzurePushNotification : PushNotificationApiService
    {
        private AzureSnsConfiguration _config;
        private DataContext _context;

        public AzurePushNotification(AzureSnsConfiguration config, DataContext context)
        {
            _config = config;
            _context = context;
        }

        public override async Task<ServiceResult<string>> RegisterPushTokenToSNSAsync(string pushToken, PushNotificationProvider provider)
        {
            var hubClient = NotificationHubClient.CreateClientFromConnectionString(_config.ConnectionString, _config.HubName);

            string azureRegistrationId = null;
            //gets registrations for this pushtoken
            var existingRegistrations = hubClient.GetRegistrationsByChannelAsync(pushToken, 100).Result.ToArray();
            if (existingRegistrations.Length > 0)
            {
                azureRegistrationId = existingRegistrations.Select(r => r.RegistrationId).First();

                //Deletes duplicates, if any
                foreach (var duplicate in existingRegistrations.Skip(1))
                    hubClient.DeleteRegistrationAsync(duplicate).Wait();
            }
            else
            {
                azureRegistrationId = await hubClient.CreateRegistrationIdAsync();
            }

            //creates new pushTokenId
            var pushTokenId = $"{provider.GetData<string>("Provider")}:{_config.HubName}:{Guid.NewGuid()}";
            RegistrationDescription registration = null;
            if (provider == PushNotificationProvider.Google)
            {
                registration = new FcmRegistrationDescription(pushToken, new string[] { pushTokenId });
            }
            else
            {
                registration = new AppleRegistrationDescription(pushToken, new string[] { pushTokenId });
            }

            registration.RegistrationId = azureRegistrationId;
            registration = await hubClient.CreateOrUpdateRegistrationAsync(registration);

            if (registration.Tags.Contains(pushTokenId))
            {
                return ServiceResult<string>.AsSuccess(pushTokenId);
            }
            else
            {
                return ServiceResult<string>.AsError(error: $"Error registering push token: {registration.Serialize()}");
            }
        }

        public async override Task<ServiceResult> SendPushNotification(SendNotificationModel model)
        {
            var record = PushNotification.CreateFrom(model);

            try
            {
                var nativeMsg = CreateNativeMessage(model, record.Provider);
                record.Message = JsonConvert.SerializeObject(nativeMsg, Formatting.None);
                _context.PushNotifications.Add(record);
                await _context.SaveChangesAsync();

                return await SendPushNotification(record, model.PushTokenId);
            }
            catch (Exception ex)
            {
                ex.LogException();
                record.Status = PushNotificationStatus.Error;
                record.Error = ex.Message;
                await _context.SaveChangesAsync();
                return ServiceResult.AsError(ex.Message);
            }
        }

        private async Task<ServiceResult> SendPushNotification(PushNotification record, string pushTokenId)
        {
            NotificationOutcome outcome = null;
            var hubClient = NotificationHubClient.CreateClientFromConnectionString(_config.ConnectionString, _config.HubName, enableTestSend: false);

            if (record.Provider == PushNotificationProvider.Google)
            {
                outcome = await hubClient.SendFcmNativeNotificationAsync(record.Message, pushTokenId);
            }
            else
            {
                outcome = await hubClient.SendAppleNativeNotificationAsync(record.Message, pushTokenId);
            }
            var state = outcome.State;

            //Doesn't work on free tier
            if (String.IsNullOrEmpty(outcome.NotificationId))
            {
                state = NotificationOutcomeState.Completed;
            }
            else
            {
                //TODO move this to a background task
                var feedbackUri = string.Empty;
                var retryCount = 0;
                while (retryCount++ < 6)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    var result = await hubClient.GetNotificationOutcomeDetailsAsync(outcome.NotificationId);
                    if (result.State != NotificationOutcomeState.Enqueued && result.State != NotificationOutcomeState.Processing)
                    {
                        feedbackUri = result.PnsErrorDetailsUri;
                        state = result.State;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(feedbackUri))
                {
                    //Console.WriteLine("feedbackBlobUri: {0}", feedbackUri); var feedbackFromBlob = ReadFeedbackFromBlob(new Uri(feedbackUri)); Console.WriteLine("Feedback from blob: {0}", feedbackFromBlob);
                }
            }

            record.Status = PushNotificationStatus.Queued;
            if (state == NotificationOutcomeState.Completed)
            {
                record.Status = PushNotificationStatus.Sent;
            }
            else if (outcome.Failure > 0 || state == NotificationOutcomeState.NoTargetFound)
            {
                record.Status = PushNotificationStatus.Error;
                if (state == NotificationOutcomeState.NoTargetFound) record.Error = "Target not found";
                else if (outcome.Results.Any()) record.Error = outcome.Results.First().Outcome;
                await _context.SaveChangesAsync();
                return ServiceResult.AsError("Failed to send");

            }
            await _context.SaveChangesAsync();
            return ServiceResult.AsSuccess();
        }

        public async override Task<ServiceResult> QueuePushNotification(SendNotificationModel model)
        {
            var record = PushNotification.CreateFrom(model);
            record.Status = PushNotificationStatus.QueuedInternally;
            var nativeMsg = CreateNativeMessage(model, record.Provider);
            record.Message = JsonConvert.SerializeObject(nativeMsg, Formatting.None);
            _context.PushNotifications.Add(record);
            await _context.SaveChangesAsync();
            return ServiceResult.AsSuccess();
        }

        public async override Task QueuePushNotificationTask(ITaskExecutionInfo executionInfo)
        {
            var record = await _context.PushNotifications
                .Include(x => x.UserDevice)
                .Where(x => x.Status == PushNotificationStatus.QueuedInternally)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            if (record == null) return;
            record.Status = PushNotificationStatus.Initialising;
            await _context.SaveChangesAsync();

            await SendPushNotification(record, record.UserDevice.PushTokenId);
        }

        protected override bool IsSandBox()
        {
            return _config.SandBox;
        }
    }
}

namespace ChilliCoreTemplate.Service.Api.FireBase
{
    using FirebaseAdmin.Messaging;
    using Microsoft.EntityFrameworkCore;

    internal class FireBasePushNotification : PushNotificationApiService
    {
        private FireBaseConfiguration _config;
        private DataContext _context;

        public FireBasePushNotification(FireBaseConfiguration config, DataContext context)
        {
            _config = config;
            _context = context;
        }

        protected override bool IsSandBox()
        {
            return false;
        }

        private FirebaseMessaging CreateSNSClient()
        {
            return FirebaseMessaging.DefaultInstance;
        }

        public async override Task<ServiceResult> SendPushNotification(SendNotificationModel model)
        {
            var record = PushNotification.CreateFrom(model);

            try
            {
                if (String.IsNullOrWhiteSpace(model.PushTokenId)) return ServiceResult.AsError("Push token id is required");

                var snsClient = CreateSNSClient();

                var message = new Message()
                {
                    Data = model.Data,
                    Notification = new Notification
                    {
                        Title = model.Title,
                        Body = model.Message
                    },
                    Token = model.PushTokenId
                };
                record.Message = message.ToJson();
                _context.PushNotifications.Add(record);
                await _context.SaveChangesAsync();

                var result = await snsClient.SendAsync(message);

                record.Status = PushNotificationStatus.Queued;
                record.MessageId = result;
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                if (!(ex is EndpointDisabledException) && !(ex is InvalidParameterException)) ex.LogException();
                if (!(ex is DbUpdateException))
                {
                    record.Status = PushNotificationStatus.Error;
                    record.Error = ex.Message;
                    await _context.SaveChangesAsync();
                }
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


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<ServiceResult<string>> RegisterPushTokenToSNSAsync(string pushToken, PushNotificationProvider provider)
        {
            return ServiceResult<string>.AsSuccess(pushToken);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

}
