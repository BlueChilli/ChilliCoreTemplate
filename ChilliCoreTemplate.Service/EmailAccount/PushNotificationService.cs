using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using System;
using System.Linq;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService
    {
        public static void PushNotification_LinqMapper()
        {
            LinqMapper.CreateMap<PushNotification, PushNotificationSummaryModel>((PushNotification x) =>
                new PushNotificationSummaryModel
                {
                    Recipient = x.UserId == null ? null : x.User.FullName,
                    _Type = x.Type,
                    Status = x.Status.ToString()
                });
            Materializer.RegisterAfterMap<PushNotificationSummaryModel>(x =>
            {
                x.Type = x._Type.GetDescription();
            });
            LinqMapper.CreateMap<PushNotification, PushNotificationDetailModel>();

        }

        public PagedList<PushNotificationSummaryModel> PushNotification_Search(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = dateFrom.FromUserTimezone();
            dateTo = dateTo.FromUserTimezone().Add(new TimeSpan(23, 59, 59));
            //var templateQuery = model.Columns.First(c => c.Data == "Type").Search.Value;
            //var templateQueryHash = String.IsNullOrEmpty(templateQuery) ? 0 : templateQuery.GetIndependentHashCode();
            var isOpenedValue = model.Columns.First(c => c.Field == "isOpened").Search.Value;
            bool? isOpened = String.IsNullOrEmpty(isOpenedValue) ? null : bool.Parse(isOpenedValue).ToNullable<bool>();
            var isSentdValue = model.Columns.First(c => c.Field == "isSent").Search.Value;
            bool? isSent = String.IsNullOrEmpty(isSentdValue) ? null : bool.Parse(isSentdValue).ToNullable<bool>();

            var query = Context.PushNotifications
                    .Where(x => (x.CreatedOn > dateFrom && x.CreatedOn < dateTo));

            if (!String.IsNullOrEmpty(model.Search.Value)) query = query.Where(x => x.User.FullName.Contains(model.Search.Value));

            if (isSent.HasValue) query = query.Where(x => x.IsSent == isSent.Value);
            if (isOpened.HasValue) query = query.Where(x => x.IsOpened == isOpened.Value);

            return query
                .OrderByDescending(x => x.Id)
                .Materialize<PushNotification, PushNotificationSummaryModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);
        }

        public int PushNotification_Count()
        {
            return Context.PushNotifications.Count();
        }

        public ServiceResult<PushNotificationDetailModel> PushNotification_Get(int id)
        {
            var notification = Context.PushNotifications
                .Where(x => x.Id == id)
                .Materialize<PushNotification, PushNotificationDetailModel>()
                .FirstOrDefault();

            return ServiceResult<PushNotificationDetailModel>.AsSuccess(notification);
        }
    }

}
