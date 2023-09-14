using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class BulkImportService : IService
    {
        private readonly DataContext Context;
        private readonly IServiceScopeFactory _scopeFactory;

        public BulkImportService(DataContext context, IServiceScopeFactory scopeFactory) 
        {
            Context = context;
            _scopeFactory = scopeFactory;
        }

        internal static void AutoMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BulkImport, BulkImportViewModel>()
                .ForMember(dest => dest.StartedOn, opt => opt.MapFrom((src, dest, destMember, ctx) => src.StartedOn == null ? null : src.StartedOn.Value.ToTimezone((string)ctx.Items["Timezone"]).ToNullable<DateTime>()))
                .ForMember(dest => dest.FinishedOn, opt => opt.MapFrom((src, dest, destMember, ctx) => src.FinishedOn == null ? null : src.FinishedOn.Value.ToTimezone((string)ctx.Items["Timezone"]).ToNullable<DateTime>()));
        }

        public static List<BulkImportViewModel> BulkImport_List(DataContext context, BulkImportType type, int? companyId = null, string timezone = Constants.DefaultTimezone)
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            var query = context.BulkImports
                .Where(x => x.Type == type && (x.StartedOn == null || x.StartedOn > oneWeekAgo));

            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);

            return Mapper.Map<List<BulkImportViewModel>>(query
                .Where(x => x.Type == type && (x.StartedOn == null || x.StartedOn > oneWeekAgo))
                .OrderByDescending(x => x.StartedOn == null ? DateTime.MaxValue : x.StartedOn)
                .ToList(), opt => opt.Items["Timezone"] = timezone);
        }

        public async Task Execute(ITaskExecutionInfo executionInfo)
        {
            try
            {
                var next = await Context.BulkImports
                    .Where(x => x.StartedOn == null)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    next.StartedOn = DateTime.UtcNow;
                    await Context.SaveChangesAsync();

                    if (executionInfo != null)
                    {
                        executionInfo.SendAliveSignal();
                        if (executionInfo.IsCancellationRequested)
                            return;
                    }

                    if (next.Type == BulkImportType.EmailUser)
                    {
                        //using (var scope = _scopeFactory.CreateScope())
                        //{
                        //    var scopedService = scope.ServiceProvider.GetRequiredService<AccountService>();
                        //    await scopedService.Email_Unsubscribe_ImportTask(next, executionInfo);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        public async Task CleanUp(ITaskExecutionInfo executionInfo)
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                var old = await Context.BulkImports
                    .Where(x => x.StartedOn < thirtyDaysAgo || (x.FinishedOn == null && x.StartedOn < oneHourAgo))
                    .Take(200)
                    .ToListAsync();

                if (executionInfo != null)
                {
                    executionInfo.SendAliveSignal();
                    if (executionInfo.IsCancellationRequested)
                        return;
                }

                Context.BulkImports.RemoveRange(old);
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

    }
}

