using System.Collections.Generic;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core.Distributed;
using ChilliCoreTemplate.Service.EmailAccount;

namespace ChilliCoreTemplate.Service.Api.PushNotifications;

public abstract class PushNotificationApiServiceBase
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
    protected abstract bool IsSandBox(SendNotificationModel model);

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
                    notification = new
                        { title = model.Title, body = model.Message, sound = model.Sound, badge = model.BadgeCount },
                    data = model.Data,
                    mode = (int)model.Type
                };

                return googleMessage;
            default:
                object defaultMessage = new
                {
                    aps = new
                    {
                        alert = new { title = model.Title, body = model.Message },
                        sound = model.Sound,
                        badge = model.BadgeCount
                    },
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

    internal async Task BulkQueuePushNotification(List<int> accountIds, AccountService accountSvc, string message, Dictionary<string, string> data = null)
    {
        var devices = accountSvc.UserDevice_List(accountIds);

        foreach (var device in devices)
        {
            var model = new SendNotificationModel
            {
                UserId = device.UserId,
                UserDeviceId = device.UserDeviceId,
                Provider = device.Provider,
                Type = PushNotificationType.Test,
                Title = null,
                Message = message,
                Data = data
            };
            await QueuePushNotification(model);
        }
    }

}