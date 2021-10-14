using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Sms;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChilliCoreTemplate.Service.Admin
{
    public partial class AdminService
    {

        public static void Sms_LinqMapper()
        {
            LinqMapper.CreateMap<SmsQueueItem, SmsViewModel>(x => new SmsViewModel
            {
                _Data = x.Data
            });

            LinqMapper.CreateMap<SmsQueueItem, SmsSummaryModel>(x => new SmsSummaryModel
            {
                _Data = x.Data,
                IsSent = x.SentOn != null,
                IsDelivered = x.DeliveredOn != null,
                IsClicked = x.ClickedOn != null
            });
            Materializer.RegisterAfterMap<SmsSummaryModel>((x) =>
            {
                x.QueuedOnDisplay = x.QueuedOn.ToTimezone().ToIsoDateTime();
            });
        }

        public ServiceResult<SmsViewModel> Sms_Get(int id)
        {
            var sms = Context.SmsQueue
                .Where(x => x.Id == id)
                .Materialize<SmsQueueItem, SmsViewModel>()
                .FirstOrDefault();

            return ServiceResult<SmsViewModel>.AsSuccess(sms);
        }

        public PagedList<SmsSummaryModel> Sms_Search(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = dateFrom.FromUserTimezone();
            dateTo = dateTo.FromUserTimezone().Add(new TimeSpan(23, 59, 59));
            var templateQuery = model.Columns.First(c => c.Field == "templateId").Search.Value;
            var templateQueryHash = String.IsNullOrEmpty(templateQuery) ? 0 : templateQuery.GetIndependentHashCode();
            var isDeliveredValue = model.Columns.First(c => c.Field == "isDelivered").Search.Value;
            bool? isDelivered = String.IsNullOrEmpty(isDeliveredValue) ? null : bool.Parse(isDeliveredValue).ToNullable<bool>();
            var isClickedValue = model.Columns.First(c => c.Field == "isClicked").Search.Value;
            bool? isClicked = String.IsNullOrEmpty(isClickedValue) ? null : bool.Parse(isClickedValue).ToNullable<bool>();

            var query = Context.SmsQueue.Where(e => e.QueuedOn > dateFrom && e.QueuedOn < dateTo);

            if (!String.IsNullOrEmpty(model.Search.Value)) query = query.Where(e => e.Data.Contains(model.Search.Value));
            if (templateQueryHash != 0) query = query.Where(e => e.TemplateIdHash == templateQueryHash);
            if (isDelivered != null)
                if (isDelivered.Value) query = query.Where(e => e.DeliveredOn != null);
                else query = query.Where(e => e.DeliveredOn == null);

            if (isClicked != null)
                if (isClicked.Value) query = query.Where(e => e.ClickedOn != null);
                else query = query.Where(e => e.ClickedOn == null);

            var sortColumn = model.Columns.FirstOrDefault(c => c.Sort != null);
            var queryOrdered = sortColumn.Sort?.Direction == SortDirection.Ascending
                ? query.OrderBy(e => e.QueuedOn)
                : query.OrderByDescending(e => e.QueuedOn);

            return queryOrdered
                .Materialize<SmsQueueItem, SmsSummaryModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);
        }

        public int Sms_Count()
        {
            return Context.SmsQueue.Count();
        }

    }
}
