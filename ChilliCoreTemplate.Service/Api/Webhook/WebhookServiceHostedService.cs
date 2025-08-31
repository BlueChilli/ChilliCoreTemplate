using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service.Api;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api;

/// <summary>
/// Hosted background service that monitors the WebhooksInbound database table for new, unprocessed webhook entries.
/// Processes webhooks in batches, handling those with unique non-null SubtypeId values in parallel, and those with duplicate or null SubtypeId values sequentially to avoid conflicts.
/// Marks each webhook as processed and logs errors as appropriate.
/// </summary>
public class WebhookServiceHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private const int BatchSize = 20;
    private const int PollIntervalMs = 2000;
    private const int WarmUpDelayMs = 5000;

    public WebhookServiceHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var env = _serviceProvider.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            var config = _serviceProvider.GetRequiredService<ProjectSettings>();
            if (config.DisableTasks) return;
        }
        else
        {
            await Task.Delay(WarmUpDelayMs, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            bool processedAny = false;

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                var items = await db.WebhooksInbound
                    .Where(w => !w.Processed)
                    .OrderBy(w => w.Id)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                if (items.Count > 0)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    // Group by Subtype
                    var nonNullSubtypeGroups = items
                        .Where(x => !string.IsNullOrEmpty(x.SubtypeId))
                        .GroupBy(x => x.SubtypeId)
                        .ToList();

                    // Items with unique (distinct) non-null Subtype
                    var parallelItems = nonNullSubtypeGroups
                        .Where(g => g.Count() == 1)
                        .SelectMany(g => g)
                        .ToList();

                    // Items with duplicate Subtype in batch or null/empty Subtype
                    var sequentialItems = nonNullSubtypeGroups
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g)
                        .Concat(items.Where(x => string.IsNullOrEmpty(x.SubtypeId)))
                        .ToList();

                    // Process parallel items
                    var parallelTasks = parallelItems
                        .Select(item => ProcessWebhookItemAsync(item, _serviceProvider, env, stoppingToken))
                        .ToList();

                    // Process sequential items
                    foreach (var item in sequentialItems)
                    {
                        await ProcessWebhookItemAsync(item, _serviceProvider, env, stoppingToken);
                    }

                    // Await all parallel tasks
                    if (parallelTasks.Count > 0)
                    {
                        await Task.WhenAll(parallelTasks);
                    }

                    processedAny = true;
                }
            }

            if (!processedAny)
            {
                await Task.Delay(PollIntervalMs, stoppingToken);
            }
        }
    }

    private static async Task ProcessWebhookItemAsync(
        WebhookInbound item,
        IServiceProvider serviceProvider,
        IWebHostEnvironment env,
        CancellationToken stoppingToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var result = ServiceResult.AsSuccess();
            try
            {
                result = await WebhookService.ProcessWebhook(item, serviceProvider);
            }
            catch (Exception ex)
            {
                ex.LogException();
                result.Success = false;
                result.Error = ex.Message;
            }

            // Attach and update the entity in the new context
            db.Attach(item);
            item.Processed = true;
            item.ProcessedOn = DateTime.UtcNow;
            item.Success = result.Success;
            item.Error = string.IsNullOrEmpty(result.Error) ? null : result.Error;

            try
            {
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                ex.LogException();
            }

            if (!result.Success)
            {
                ErrorLogHelper.LogMessage($"{item.Type} Webhook failed: {result.Error}", env.IsProduction() ? LogEventLevel.Error : LogEventLevel.Warning);
            }
        }
    }
}