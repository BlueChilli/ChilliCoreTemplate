using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api
{
    public class ApiLogService : Service<DataContext>
    {
        private readonly ProjectSettings _config;

        public ApiLogService(BackgroundTaskPrincipal user, DataContext context, ProjectSettings config) : base(user, context)
        {
            _config = config;
        }

        public Task SaveAsync(ApiLogEntry model)
        {
            Context.ApiLogEntries.Add(model);
            return Context.SaveChangesAsync();
        }

        public async Task Clean(ITaskExecutionInfo executionInfo)
        {
            if (executionInfo != null)
            {
                executionInfo.SendAliveSignal();
                if (executionInfo.IsCancellationRequested)
                    return;
            }

            Context.Database.SetCommandTimeout(TimeSpan.FromSeconds(60));
            await Context.Database.ExecuteSqlRawAsync($"DELETE FROM[dbo].[ApiLogEntries] WHERE Id IN (SELECT TOP(50) Id FROM [dbo].[ApiLogEntries] ORDER BY Id) AND [RequestTimestamp] < DATEADD(day, -14, SYSUTCDATETIME())");
        }

        internal async Task SaveAsync(RestClient client, RestResponse response)
        {
            if (!_config.ApiSettings.LogApiCalls) return;

            var headersIn = response.Request.Parameters.Where(x => x.Type == ParameterType.HttpHeader).ToDictionary(k => k.Name, k => k.Value.ToString());
            foreach (var header in client.DefaultParameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                headersIn.Add(header.Name, header.Value.ToString());
            }

            var headersOut = new Dictionary<string, List<string>>();
            if (response.Headers != null)
            {
                var keys = response.Headers.Select(x => x.Name).Distinct().ToList();
                foreach (var key in keys)
                {
                    headersOut[key] = response.Headers.Where(x => x.Name == key).Select(x => x.Value.ToString()).ToList();
                }
            }

            var contentType = response.Request.Parameters.Where(x => x.Name == "Content-Type").Select(x => x.Value.ToString()).FirstOrDefault();
            var body = response.Request.Parameters.Where(x => x.Type == ParameterType.RequestBody).FirstOrDefault();

            var model = new ApiLogEntry
            {
                User = User?.Identity?.Name,
                Machine = Environment.MachineName,
                RequestIpAddress = "127.0.0.1",
                RequestContentType = contentType ??= body?.ContentType,
                RequestContentBody = body?.Value.ToJson(),
                RequestUri = client.BuildUri(response.Request).ToString(),
                RequestMethod = response.Request.Method.ToString(),
                RequestHeaders = JsonConvert.SerializeObject(headersIn, Formatting.None),
                RequestTimestamp = DateTime.UtcNow,
                ResponseContentType = response.ContentType,
                ResponseContentBody = response.Content,
                ResponseStatusCode = (int)response.StatusCode,
                ResponseHeaders = JsonConvert.SerializeObject(headersOut, Formatting.None),
                ResponseTimestamp = DateTime.UtcNow
            };
            await SaveAsync(model);
        }
    }

}
