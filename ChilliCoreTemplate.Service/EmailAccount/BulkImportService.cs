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
using ChilliSource.Core.Extensions;
using System.Security.Principal;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using ChilliSource.Cloud.Core.LinqMapper;
using System.IO.Compression;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class BulkImportServiceAutoMapperConfig : Profile
    {
        public BulkImportServiceAutoMapperConfig()
        {
            CreateMap<BulkImport, BulkImportViewModel>()
                .ForMember(dest => dest.StartedOn, opt => opt.MapFrom((src, dest, destMember, ctx) => src.StartedOn == null ? null : src.StartedOn.Value.ToTimezone((string)ctx.Items["Timezone"]).ToNullable<DateTime>()))
                .ForMember(dest => dest.FinishedOn, opt => opt.MapFrom((src, dest, destMember, ctx) => src.FinishedOn == null ? null : src.FinishedOn.Value.ToTimezone((string)ctx.Items["Timezone"]).ToNullable<DateTime>()));
        }
    }

    public class BulkImportService : BaseService
    {
        public const string Folder = "BulkImport";
        private readonly IServiceScopeFactory _scopeFactory;

        public BulkImportService(IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment, IServiceScopeFactory scopeFactory, IMapper mapper)
            : base(user, context, config, fileStorage, environment, mapper)
        {
            _scopeFactory = scopeFactory;
        }

        internal static void LinqMapperConfigure()
        {
            LinqMapper.CreateMap<BulkImport, BulkImportViewModel>(x => new BulkImportViewModel
            {
                CanDownload = x.FilesJson != null
            });
            Materializer.RegisterAfterMap<BulkImportViewModel>((x) =>
            {
                x.QueuedOn = x.QueuedOn.ToTimezone(x.CompanyTimezone ?? Constants.DefaultTimezone);
                if (x.StartedOn.HasValue) x.StartedOn = x.StartedOn.Value.ToTimezone(x.CompanyTimezone ?? Constants.DefaultTimezone);
                if (x.FinishedOn.HasValue) x.FinishedOn = x.FinishedOn.Value.ToTimezone(x.CompanyTimezone ?? Constants.DefaultTimezone);
            });
        }

        public IQueryable<BulkImport> Authorised()
        {
            if (IsAdmin) return Context.BulkImports.AsQueryable();

            return Context.BulkImports.Where(x => x.CompanyId == CompanyId.Value);
        }

        public ServiceResult<BulkImportListModel> List()
        {
            var records = Authorised()
                .Materialize<BulkImport, BulkImportViewModel>()
                .ToList();
            return ServiceResult<BulkImportListModel>.AsSuccess(new BulkImportListModel { Imports = records });
        }

        public ServiceResult<MemoryStream> Download(int id)
        {
            var record = Authorised().Where(x => x.Id == id).FirstOrDefault();

            if (record == null) return ServiceResult<MemoryStream>.AsError("Bulk import not found");

            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var files = record.Files();

                foreach (var file in files)
                {
                    if (_fileStorage.Exists(file.FilePath))
                    {
                        var data = _fileStorage.GetContent(file.FilePath).ReadToByteArray();
                        var entry = archive.CreateEntry(file.FileName);
                        entry.AddByteArray(data);
                    }
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            return ServiceResult<MemoryStream>.AsSuccess(memoryStream);
        }

        public static List<BulkImportViewModel> List(DataContext context, IMapper mapper, BulkImportType type, int? companyId = null, string timezone = Constants.DefaultTimezone)
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            var query = context.BulkImports
                .Where(x => x.Type == type && (x.StartedOn == null || x.StartedOn > oneWeekAgo));

            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);

            return mapper.Map<List<BulkImportViewModel>>(query
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

