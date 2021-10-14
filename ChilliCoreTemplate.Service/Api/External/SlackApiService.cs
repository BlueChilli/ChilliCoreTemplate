using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Hosting;

namespace ChilliCoreTemplate.Service.Api.Slack
{
    public class SlackApiService : IService
    {
        readonly DataContext Context;
        readonly ProjectSettings _config;
        private readonly IWebHostEnvironment _environment;

        public SlackApiService(DataContext context, ProjectSettings config, IWebHostEnvironment environment) 
        {
            Context = context;
            _config = config;
            _environment = environment;
        }

        public void Channel_Post_Task(ITaskExecutionInfo executionInfo)
        {
            if (executionInfo != null)
            {
                executionInfo.SendAliveSignal();
                if (executionInfo.IsCancellationRequested)
                    return;
            }

            if (_config.SlackSettings.Enabled)
            {
                var dateToCheckFrom = DateTime.UtcNow.AddDays(-1);
                var loginsIn24Hours = Context.Users.Count(x => x.LastLoginDate >= dateToCheckFrom);
                var accountsIn24Hours = Context.Users.Count(x => x.CreatedDate >= dateToCheckFrom);
                var companyAccountsIn24Hours = Context.Users.Count(x => x.CreatedDate >= dateToCheckFrom && x.UserRoles.Any(r => r.CompanyId != null));

                Channel_Post(new SlackMessage
                {
                    Text = $"\n" +
                    $"*Daily Updates* \n" +
                    $"> {loginsIn24Hours} logins.\n" +
                    $"> {accountsIn24Hours} total account registrations.\n" +
                    $"> {companyAccountsIn24Hours} company account registrations.\n"
                });
            }
        }

        public void Channel_Post(SlackMessage message)
        {
            try
            {
                if (_config.SlackSettings.Enabled)
                {
                    using (var wb = new WebClient())
                    {
                        // Add environment name to the message
                        message.Text += $"\n_({_environment.EnvironmentName})_";

                        // Set headers
                        wb.Headers["Content-Type"] = "application/json";
                        var response = wb.UploadString(_config.SlackSettings.WebhookUrl, "POST", JsonConvert.SerializeObject(message));
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

    }

    public class SlackMessage
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
