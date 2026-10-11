using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service.EmailAccount;

public class PushNotificationService : BaseService
{
    public PushNotificationService(IPrincipal user, DataContext context, Models.ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment, IMapper mapper) : base(user, context, config, fileStorage, environment, mapper)
    {
    }

    public static void LinqMapper()
    {
        ChilliSource.Cloud.Core.LinqMapper.LinqMapper.CreateMap<PushNotification, PushNotificationSummaryModel>((PushNotification x) =>
            new PushNotificationSummaryModel
            {
                Recipient = x.UserId == null ? null : x.User.FullName,
                _Type = x.Type
            });
        Materializer.RegisterAfterMap<PushNotificationSummaryModel>(x =>
        {
            x.Type = x._Type.GetDescription();
        });
        ChilliSource.Cloud.Core.LinqMapper.LinqMapper.CreateMap<PushNotification, PushNotificationDetailModel>();

    }

    public PagedList<PushNotificationSummaryModel> List(IDataTablesRequest model, PushNotificationListModel filterModel)
    {
        var dateFrom = filterModel.DateFrom;
        var dateTo = filterModel.DateTo;
        dateFrom = dateFrom.Value.FromUserTimezone();
        dateTo = dateTo.Value.FromUserTimezone().Add(new TimeSpan(23, 59, 59));
        var typeValue = model.Columns.First(c => c.Field == "type").Search.Value;
        PushNotificationType? type = String.IsNullOrEmpty(typeValue) ? null : EnumHelper.Parse<PushNotificationType>(typeValue);
        var isOpenedValue = model.Columns.First(c => c.Field == "isOpened").Search.Value;
        bool? isOpened = String.IsNullOrEmpty(isOpenedValue) ? null : bool.Parse(isOpenedValue).ToNullable<bool>();
        var isSentdValue = model.Columns.First(c => c.Field == "isSent").Search.Value;
        bool? isSent = String.IsNullOrEmpty(isSentdValue) ? null : bool.Parse(isSentdValue).ToNullable<bool>();

        var scope = filterModel.UserId == null ? Context.PushNotifications : Context.PushNotifications.Where(x => x.UserId == filterModel.UserId.Value);
        var query = Context.PushNotifications
                .Where(x => (x.CreatedOn > dateFrom && x.CreatedOn < dateTo));

        if (!String.IsNullOrEmpty(model.Search.Value)) query = query.Where(x => x.User.FullName.Contains(model.Search.Value));

        if (type.HasValue) query = query.Where(x => x.Type == type.Value);
        if (isSent.HasValue) query = isSent.Value ? query.Where(x => x.Status == PushNotificationStatus.Sent) : query.Where(x => x.Status != PushNotificationStatus.Sent);
        if (isOpened.HasValue) query = isOpened.Value ? query.Where(x => x.OpenedOn.HasValue) : query.Where(x => x.OpenedOn == null);

        return query
            .OrderByDescending(x => x.Id)
            .Materialize<PushNotification, PushNotificationSummaryModel>()
            .ToPagedList(model.Start / model.Length + 1, model.Length)
            .WithScope(scope, query);
    }

    public ServiceResult<PushNotificationDetailModel> Get(int id)
    {
        var notification = Context.PushNotifications
            .Where(x => x.Id == id)
            .Materialize<PushNotification, PushNotificationDetailModel>()
            .FirstOrDefault();

        return ServiceResult<PushNotificationDetailModel>.AsSuccess(notification);
    }

    public int Count(int userId)
    {
        return Context.PushNotifications
            .Where(x => x.UserId == userId)
            .Count();
    }
}
