using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService
    {

        public PagedList<ErrorLogSummaryModel> Error_Search(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = dateFrom.FromUserTimezone();
            dateTo = dateTo.FromUserTimezone().Add(new TimeSpan(23, 59, 59));

            var query = Context.ErrorLogs
            .Where(e => (e.TimeStamp > dateFrom && e.TimeStamp < dateTo));

            if (!String.IsNullOrEmpty(model.Search.Value))
            {
                query = query.Where(x => x.User.Email.Contains(model.Search.Value) || x.Message.Contains(model.Search.Value) || x.ExceptionMessage.Contains(model.Search.Value));
            }

            return query
                .OrderByDescending(e => e.TimeStamp)
                .Materialize<ErrorLog, ErrorLogSummaryModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);
        }

        public int Error_Count()
        {
            return Context.ErrorLogs.Count();
        }

        public ServiceResult<ErrorLogExpandedModel> Error_Get(int id)
        {
            var errorDetails = Context.ErrorLogs.Where(x => x.Id == id).Select(x => new { x.Exception, x.LogEvent }).First();

            var properties = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                errorDetails.LogEvent, 
                new JsonConverter[] { new NestedJsonConverter() }
                );

            return ServiceResult<ErrorLogExpandedModel>.AsSuccess(new ErrorLogExpandedModel
            {
                Exception = errorDetails.Exception,
                Properties = properties["Properties"] as IDictionary<string, object>
            });
        }

        //https://stackoverflow.com/questions/6416017/json-net-deserializing-nested-dictionaries
        class NestedJsonConverter : CustomCreationConverter<IDictionary<string, object>>
        {
            public override IDictionary<string, object> Create(Type objectType)
            {
                return new Dictionary<string, object>();
            }

            public override bool CanConvert(Type objectType)
            {
                // in addition to handling IDictionary<string, object>
                // we want to handle the deserialization of dict value
                // which is of type object
                return objectType == typeof(object) || base.CanConvert(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartObject
                    || reader.TokenType == JsonToken.Null)
                    return base.ReadJson(reader, objectType, existingValue, serializer);

                // if the next token is not an object
                // then fall back on standard deserializer (strings, numbers etc.)
                return serializer.Deserialize(reader);
            }
        }

        private static int ErrorsSent = 0;
        private static bool DailySent = false;
        private static DateTime? NextErrorEmailDate = null;
        public async Task Error_EmailAsync(ITaskExecutionInfo executionInfo)
        {
            var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
            var previousDate = DateTime.MinValue;
            if (NextErrorEmailDate.HasValue && NextErrorEmailDate.Value > tenMinutesAgo) return;

            var config = _config.ErrorLogSettings;
            if (config == null || !config.Enabled) return;

            var errors = await Context.ErrorLogs
                .AsNoTracking()
                .Where(e => e.TimeStamp > tenMinutesAgo && e.TimeStamp < DateTime.UtcNow)
                .Take(100)
                .Materialize<ErrorLog, ErrorLogSummaryModel>()
                .ToListAsync();

            if (errors.Count >= config.ErrorCount)
            {
                ErrorsSent++;
                NextErrorEmailDate = DateTime.UtcNow.AddMinutes(10 * (ErrorsSent ^ 2));
                QueueMail(RazorTemplates.ErrorAlert, config.EmailTo, new RazorTemplateDataModel<ErrorLogAlertEmail> { Data = new ErrorLogAlertEmail { Errors = errors } });
            }
            else if (ErrorsSent > 0)
            {
                ErrorsSent--;
            }

            var dailyDays = config.ErrorDays.Split(',').Select(x => EnumHelper.Parse<DayOfWeek>(x)).ToList();
            var sydneyTime = DateTime.UtcNow.ToTimezone();
            if (dailyDays.Contains(sydneyTime.DayOfWeek) && sydneyTime.Hour == 9)
            {
                if (DailySent) return;
                DailySent = true;
                var index = dailyDays.IndexOf(sydneyTime.DayOfWeek);
                index--;
                var previousDay = index < 0 ? dailyDays.Last() : dailyDays[index];
                previousDate = sydneyTime.PreviousDayOfWeek(previousDay).AddHours(sydneyTime.Hour).FromUserTimezone();
                var dailyErrors = await Context.ErrorLogs
                   .AsNoTracking()
                   .Where(e => e.TimeStamp > previousDate)
                   .Select(e => e.ExceptionMessage == null ? e.Message.Substring(0, 200) : e.ExceptionMessage.Substring(0, 200))
                   .Distinct()
                   .Take(20)
                   .ToListAsync();
                if (dailyErrors.Count > 0)
                {
                    QueueMail(RazorTemplates.ErrorDaily, config.EmailTo, new RazorTemplateDataModel<List<string>> { Data = dailyErrors });
                }
            }
            else DailySent = false;
        }

        public async Task Error_CleanAsync(ITaskExecutionInfo executionInfo)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync($"DELETE TOP (100) FROM [dbo].[ErrorLogs] WHERE [TimeStamp] < DATEADD(day, -30, SYSUTCDATETIME());");
        }

        public ServiceResult Error_Test(string test)
        {
            try
            {
                throw new Exception(test);
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
            return ServiceResult.AsSuccess();
        }


    }
}
