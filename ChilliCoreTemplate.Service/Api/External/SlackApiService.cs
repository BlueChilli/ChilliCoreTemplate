using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api.Slack;

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

    public async Task ChannelPost_Task(ITaskExecutionInfo executionInfo)
    {
        if (_config.SlackSettings.Enabled)
        {
            if (executionInfo != null)
            {
                executionInfo.SendAliveSignal();
                if (executionInfo.IsCancellationRequested)
                    return;
            }

            var dateToCheckFrom = DateTime.UtcNow.AddDays(-1);
            var loginsIn24Hours = Context.Users.Count(x => x.LastLoginDate >= dateToCheckFrom);
            var accountsIn24Hours = Context.Users.Count(x => x.CreatedDate >= dateToCheckFrom);
            var companyAccountsIn24Hours = Context.Users.Count(x => x.CreatedDate >= dateToCheckFrom && x.UserRoles.Any(r => r.CompanyId != null));

            await ChannelPostAsync(SlackChannelType.Default,
                $"\n" +
                $"*Daily Updates* \n" +
                $"> {loginsIn24Hours} logins.\n" +
                $"> {accountsIn24Hours} total account registrations.\n" +
                $"> {companyAccountsIn24Hours} company account registrations.\n");
        }
    }

    public async Task ChannelPostAsync(string text)
    {
        await ChannelPostAsync(SlackChannelType.Default, text);
    }

    public async Task ChannelPostAsync(SlackChannelType channel, string text)
    {
        try
        {
            if (_config.SlackSettings.Enabled)
            {
                var message = CreateMessage(channel, text);
                var client = new HttpClient();
                var response = await client.SendAsync(message);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            ex.LogException();
        }
    }

    public void ChannelPost(SlackChannelType channel, string text)
    {
        try
        {
            if (_config.SlackSettings.Enabled)
            {
                var message = CreateMessage(channel, text);
                var client = new HttpClient();
                var response = client.Send(message);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            ex.LogException();
        }
    }

    private HttpRequestMessage CreateMessage(SlackChannelType channel, string text)
    {
        var message = new SlackMessage { Text = text };
        if (!_environment.IsProduction()) //Add environment name to the message
        {
            message.Text += $"\n_({_environment.EnvironmentName})_";
        }
        var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
        var hook = _config.SlackSettings.Webhooks.Where(x => x.Type == channel).First();
        return new HttpRequestMessage(HttpMethod.Post, hook.Url) { Content = content };
    }
}

public class SlackMessage
{
    [JsonProperty("text")]
    public string Text { get; set; }
}
