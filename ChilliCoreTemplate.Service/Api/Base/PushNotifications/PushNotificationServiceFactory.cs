using System;
using System.Linq;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using Microsoft.EntityFrameworkCore;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service;

namespace ChilliCoreTemplate.Service.Api.PushNotifications;

public class PushNotificationServiceFactory: IService
{
    private readonly PushNotificationSettings _settings;
    private readonly DataContext _context;

    public PushNotificationServiceFactory(PushNotificationSettings settings, DataContext context)
    {
        _settings = settings;
        _context = context;
    }

    public PushNotificationApiServiceBase GetService(PushNotificationAppId appId = PushNotificationAppId.Default)
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