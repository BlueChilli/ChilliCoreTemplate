using Amazon.SimpleNotificationService.Model;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Core.Extensions;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api.PushNotifications;


internal class FireBasePushNotification : PushNotificationApiServiceBase
{
    private FireBaseConfiguration _config;
    private DataContext _context;

    public FireBasePushNotification(FireBaseConfiguration config, DataContext context)
    {
        _config = config;
        _context = context;
    }

    protected override bool IsSandBox(SendNotificationModel model)
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
