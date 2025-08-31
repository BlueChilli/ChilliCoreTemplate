using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Core.Extensions;
using Microsoft.Azure.NotificationHubs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api.PushNotifications;


internal class AzurePushNotification : PushNotificationApiServiceBase
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

    protected override bool IsSandBox(SendNotificationModel model)
    {
        return _config.SandBox;
    }
}

